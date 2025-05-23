// (c) Copyright Ascensio System SIA 2009-2025
// 
// This program is a free software product.
// You can redistribute it and/or modify it under the terms
// of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
// Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
// to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of
// any third-party rights.
// 
// This program is distributed WITHOUT ANY WARRANTY, without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see
// the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
// 
// You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
// 
// The  interactive user interfaces in modified source and object code versions of the Program must
// display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
// 
// Pursuant to Section 7(b) of the License you must retain the original Product logo when
// distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under
// trademark law for use of our trademarks.
// 
// All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
// content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
// International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode

namespace ASC.Data.Backup.Tasks;

[Scope]
public class TransferPortalTask(DbFactory dbFactory,
        IServiceProvider serviceProvider,
        ILogger<TransferPortalTask> options,
        StorageFactory storageFactory,
        StorageFactoryConfig storageFactoryConfig,
        ModuleProvider moduleProvider,
        TempStream tempStream,
        IFusionCache cache)
    : PortalTaskBase(dbFactory, options, storageFactory, storageFactoryConfig, moduleProvider)
{
    public const string DefaultDirectoryName = "backup";

    public string BackupDirectory { get; set; }
    public bool DeleteBackupFileAfterCompletion { get; set; } = true;
    public bool BlockOldPortalAfterStart { get; set; } = true;
    public bool DeleteOldPortalAfterCompletion { get; set; } = true;
    public string ToRegion { get; private set; }
    public int ToTenantId { get; private set; }
    public int Limit { get; private set; }


    public void Init(int tenantId, string toRegion, int limit, string backupDirectory)
    {
        Limit = limit;
        ToRegion = toRegion ?? throw new ArgumentNullException(nameof(toRegion));
        Init(tenantId);

        BackupDirectory = backupDirectory;
    }

    public override async Task RunJob()
    {
        options.DebugBeginTransfer(TenantId);
        var fromDbFactory = new DbFactory(null, null);
        var toDbFactory = new DbFactory(null, null);
        var tenantAlias = GetTenantAlias(fromDbFactory);
        var backupFilePath = GetBackupFilePath(tenantAlias);
        var columnMapper = new ColumnMapper();
        try
        {
            //target db can have error tenant from the previous attempts
            SaveTenant(toDbFactory, tenantAlias, TenantStatus.RemovePending, tenantAlias + "_error", "status = " + TenantStatus.Restoring.ToString("d"));

            if (BlockOldPortalAfterStart)
            {
                SaveTenant(fromDbFactory, tenantAlias, TenantStatus.Transfering);
            }

            SetStepsCount(ProcessStorage ? 3 : 2);

            //save db data to temporary file
            var backupTask = serviceProvider.GetService<BackupPortalTask>();
            backupTask.Init(TenantId, backupFilePath, Limit, DataOperatorFactory.GetDefaultWriteOperator(tempStream, backupFilePath), false);
            backupTask.ProcessStorage = false;
            backupTask.ProgressChanged = args => SetCurrentStepProgress(args.Progress);
            foreach (var moduleName in _ignoredModules)
            {
                backupTask.IgnoreModule(moduleName);
            }
            await backupTask.RunJob();

            //restore db data from temporary file
            var restoreTask = serviceProvider.GetService<RestorePortalTask>();
            restoreTask.Init(ToRegion, backupFilePath, false, columnMapper: columnMapper);
            restoreTask.ProcessStorage = false;
            restoreTask.ProgressChanged = args => SetCurrentStepProgress(args.Progress);
            foreach (var moduleName in _ignoredModules)
            {
                restoreTask.IgnoreModule(moduleName);
            }
            await restoreTask.RunJob();

            //transfer files
            if (ProcessStorage)
            {
                await DoTransferStorageAsync(columnMapper);
            }

            SaveTenant(toDbFactory, tenantAlias, TenantStatus.Active);
            if (DeleteOldPortalAfterCompletion)
            {
                SaveTenant(fromDbFactory, tenantAlias, TenantStatus.RemovePending, tenantAlias + "_deleted");
            }
            else if (BlockOldPortalAfterStart)
            {
                SaveTenant(fromDbFactory, tenantAlias, TenantStatus.Active);
            }

            ToTenantId = columnMapper.GetTenantMapping();
        }
        catch
        {
            SaveTenant(fromDbFactory, tenantAlias, TenantStatus.Active);
            if (columnMapper.GetTenantMapping() > 0)
            {
                SaveTenant(toDbFactory, tenantAlias, TenantStatus.RemovePending, tenantAlias + "_error");
            }
            throw;
        }
        finally
        {
            if (DeleteBackupFileAfterCompletion)
            {
                File.Delete(backupFilePath);
            }
            options.DebugEndTransfer(TenantId);
        }
    }

    private async Task DoTransferStorageAsync(ColumnMapper columnMapper)
    {
        options.DebugBeginTransferStorage();
        var fileGroups = (await GetFilesToProcess(TenantId).ToListAsync()).GroupBy(file => file.Module).ToList();
        var groupsProcessed = 0;
        foreach (var group in fileGroups)
        {
            var baseStorage = await StorageFactory.GetStorageAsync(TenantId, group.Key);
            var destStorage = await StorageFactory.GetStorageAsync(columnMapper.GetTenantMapping(), group.Key, ToRegion);
            var utility = new CrossModuleTransferUtility(options, tempStream, baseStorage, destStorage, cache);

            foreach (var file in group)
            {
                var adjustedPath = file.Path;

                var module = ModuleProvider.GetByStorageModule(file.Module, file.Domain);
                if (module == null || module.TryAdjustFilePath(false, columnMapper, ref adjustedPath))
                {
                    try
                    {
                        await utility.CopyFileAsync(file.Domain, file.Path, file.Domain, adjustedPath);
                    }
                    catch (Exception error)
                    {
                        options.WarningCantCopyFile(file.Module, file.Path, error);
                    }
                }
                else
                {
                    options.WarningCantAdjustFilePath(file.Path);
                }
            }
            await SetCurrentStepProgress((int)(++groupsProcessed * 100 / (double)fileGroups.Count));
        }

        if (fileGroups.Count == 0)
        {
            await SetStepCompleted();
        }

        options.DebugEndTransferStorage();
    }

    private void SaveTenant(DbFactory dbFactory, string alias, TenantStatus status, string newAlias = null, string whereCondition = null)
    {
        using var connection = dbFactory.OpenConnection();
        if (newAlias == null)
        {
            newAlias = alias;
        }
        else if (newAlias != alias)
        {
            newAlias = GetUniqAlias(connection, newAlias);
        }

        var commandText = "update tenants_tenants " +
        "set " +
            $"  status={status:d}, " +
            $"  alias = '{newAlias}', " +
            $"  last_modified='{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', " +
            $"  statuschanged='{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}' " +
            $"where alias = '{alias}'";

        if (!string.IsNullOrEmpty(whereCondition))
        {
            commandText += " and " + whereCondition;
        }

        var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.WithTimeout(120).ExecuteNonQuery();
    }

    private string GetTenantAlias(DbFactory dbFactory)
    {
        using var connection = dbFactory.OpenConnection();
        var command = connection.CreateCommand();
        command.CommandText = "select alias from tenants_tenants where id = " + TenantId;
        return (string)command.WithTimeout(120).ExecuteScalar();
    }

    private static string GetUniqAlias(DbConnection connection, string alias)
    {
        var command = connection.CreateCommand();
        command.CommandText = "select count(*) from tenants_tenants where alias like '" + alias + "%'";
        return alias + command.WithTimeout(120).ExecuteScalar();
    }

    private string GetBackupFilePath(string tenantAlias)
    {
        if (!Directory.Exists(BackupDirectory ?? DefaultDirectoryName))
        {
            Directory.CreateDirectory(BackupDirectory ?? DefaultDirectoryName);
        }

        return CrossPlatform.PathCombine(BackupDirectory ?? DefaultDirectoryName, tenantAlias + DateTime.UtcNow.ToString("(yyyy-MM-dd HH-mm-ss)", CultureInfo.InvariantCulture) + ".backup");
    }

}
