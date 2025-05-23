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
public class RestorePortalTask(DbFactory dbFactory,
        ILogger<RestorePortalTask> options,
        ILogger<RestoreDbModuleTask> logger,
        StorageFactory storageFactory,
        StorageFactoryConfig storageFactoryConfig,
        CoreBaseSettings coreBaseSettings,
        LicenseReader licenseReader,
        TenantManager tenantManager,
        AscCacheNotify ascCacheNotify,
        ModuleProvider moduleProvider,
        BackupRepository backupRepository,
        TenantExtra tenantExtra)
    : PortalTaskBase(dbFactory, options, storageFactory, storageFactoryConfig, moduleProvider)
{
    public bool ReplaceDate { get; set; }
    public bool Dump { get; set; }
    public string BackupFilePath { get; private set; }
    public string UpgradesPath { get; private set; }
    public bool UnblockPortalAfterCompleted { get; set; }

    private ColumnMapper _columnMapper;
    private string _region;
    private bool _expectDump;

    public void Init(string region, string fromFilePath, bool expectDump, int tenantId = -1, ColumnMapper columnMapper = null, string upgradesPath = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(fromFilePath);

        if (!File.Exists(fromFilePath))
        {
            throw new FileNotFoundException("file not found at given path");
        }

        BackupFilePath = fromFilePath;
        UpgradesPath = upgradesPath;
        _columnMapper = columnMapper ?? new ColumnMapper();
        _region = region;
        _expectDump = expectDump; 
        Init(tenantId);
    }

    public override async Task RunJob()
    {
        options.DebugBeginRestorePortal();

        options.DebugBeginRestoreData();

        using (var dataReader = DataOperatorFactory.GetReadOperator(BackupFilePath))
        {
            await using (var entry = dataReader.GetEntry(KeyHelper.GetDumpKey()))
            {
                Dump = entry != null;
            }

            if (Dump && !coreBaseSettings.Standalone)
            {
                throw new ArgumentException(BackupResource.BackupNotFound);
            }

            if (Dump != _expectDump)
            {
                throw new ArgumentException(BackupResource.BackupInvalid);
            }

            if (Dump)
            {
                await tenantExtra.DemandAccessSpacePermissionAsync();
                await RestoreFromDump(dataReader);
            }
            else
            {
                var modulesToProcess = GetModulesToProcess().ToList();
                SetStepsCount(ProcessStorage ? modulesToProcess.Count + 1 : modulesToProcess.Count);

                foreach (var module in modulesToProcess)
                {
                    var restoreTask = new RestoreDbModuleTask(logger, module, dataReader, _columnMapper, DbFactory, ReplaceDate, Dump, _region, StorageFactory, StorageFactoryConfig, ModuleProvider)
                    {
                        ProgressChanged = args => SetCurrentStepProgress(args.Progress)
                    };

                    foreach (var tableName in _ignoredTables)
                    {
                        restoreTask.IgnoreTable(tableName);
                    }

                    await restoreTask.RunJob();
                }
                await backupRepository.MigrationBackupRecordsAsync(TenantId, _columnMapper.GetTenantMapping(), _region);
            }

            options.DebugEndRestoreData();

            if (ProcessStorage)
            {
                if (coreBaseSettings.Standalone)
                {
                    options.DebugClearCache();
                    await ascCacheNotify.ClearCacheAsync();
                }

                await DoRestoreStorage(dataReader);
            }

            if (UnblockPortalAfterCompleted)
            {
                SetTenantActive(_columnMapper.GetTenantMapping());
            }
        }

        if (coreBaseSettings.Standalone && Dump)
        {
            options.DebugRefreshLicense();
            try
            {
                await licenseReader.RejectLicenseAsync();
            }
            catch (Exception ex)
            {
                options.ErrorRunJob(ex);
            }

            options.DebugClearCache();
            await ascCacheNotify.ClearCacheAsync();
        }

        options.DebugEndRestorePortal();
    }

    private async Task RestoreFromDump(IDataReadOperator dataReader)
    {
        var keyBase = KeyHelper.GetDatabaseSchema();
        var keys = dataReader.GetEntries(keyBase).Select(Path.GetFileName).ToList();
        var dbs = dataReader.GetDirectories("").Where(r => Path.GetFileName(r).StartsWith("mailservice")).Select(Path.GetFileName).ToList();
        var upgrades = new List<string>();

        if (!string.IsNullOrEmpty(UpgradesPath) && Directory.Exists(UpgradesPath))
        {
            upgrades = Directory.GetFiles(UpgradesPath).ToList();
        }

        var stepscount = keys.Count * 2 + upgrades.Count;
        
        foreach (var db in dbs)
        {
            var keys1 = dataReader.GetEntries(db + "/" + keyBase).Select(Path.GetFileName).ToList();
            stepscount += keys1.Count * 2;
        }

        SetStepsCount(ProcessStorage ? stepscount + 1 : stepscount);

        if (ProcessStorage)
        {
            var storageModules = StorageFactoryConfig.GetModuleList(_region).Where(IsStorageModuleAllowed);
            var tenants = await tenantManager.GetTenantsAsync(false);

            stepscount += storageModules.Count() * tenants.Count;

            SetStepsCount(stepscount + 1);

            await DoDeleteStorageAsync(storageModules, tenants);
        }
        else
        {
            SetStepsCount(stepscount);
        }

        for (var i = 0; i < keys.Count; i += TasksLimit)
        {
            var tasks = new List<Task>(TasksLimit * 2);

            for (var j = 0; j < TasksLimit && i + j < keys.Count; j++)
            {
                var key1 = Path.Combine(KeyHelper.GetDatabaseSchema(), keys[i + j]);
                var key2 = Path.Combine(KeyHelper.GetDatabaseData(), keys[i + j]);
                tasks.Add(RestoreFromDumpFile(dataReader, key1, key2));
            }

            Task.WaitAll(tasks.ToArray());
        }

        var comparer = new SqlComparer();
        foreach (var u in upgrades.OrderBy(Path.GetFileName, comparer))
        {
            RunMysqlFile(u, true);
            await SetStepCompleted();
        }
    }

    private async Task RestoreFromDumpFile(IDataReadOperator dataReader, string fileName1, string fileName2 = null, string db = null)
    {
        options.DebugRestoreFrom(fileName1);
        await using (var stream = dataReader.GetEntry(fileName1))
        {
            await RunMysqlFile(stream, db);
        }
        await SetStepCompleted();

        options.DebugRestoreFrom(fileName2);
        if (fileName2 != null)
        {
            await using (var stream = dataReader.GetEntry(fileName2))
            {
                await RunMysqlFile(stream, db);
            }

            await SetStepCompleted();
        }
    }

    public List<object[]> ExecuteList(DbCommand command)
    {
        var list = new List<object[]>();
        using var result = command.ExecuteReader();
        while (result.Read())
        {
            var objects = new object[result.FieldCount];
            result.GetValues(objects);
            list.Add(objects);
        }

        return list;
    }

    private ConnectionStringSettings GetConnectionString(int id, string connectionString)
    {
        connectionString += ";convert zero datetime=True";
        return new ConnectionStringSettings("mailservice-" + id, connectionString, "MySql.Data.MySqlClient");
    }

    private class SqlComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y)
            {
                return 0;
            }

            if (!string.IsNullOrEmpty(x))
            {
                var splittedX = x.Split('.');
                if (splittedX.Length <= 2)
                {
                    return -1;
                }

                if (splittedX[1] == "upgrade")
                {
                    return 1;
                }

                if (splittedX[1].StartsWith("upgrade") && !string.IsNullOrEmpty(y))
                {
                    var splittedY = y.Split('.');
                    if (splittedY.Length <= 2)
                    {
                        return 1;
                    }

                    if (splittedY[1] == "upgrade")
                    {
                        return -1;
                    }

                    if (splittedY[1].StartsWith("upgrade"))
                    {
                        return string.Compare(x, y, StringComparison.Ordinal);
                    }

                    return -1;
                }

                return -1;
            }

            return string.Compare(x, y, StringComparison.Ordinal);
        }
    }

    private async Task DoRestoreStorage(IDataReadOperator dataReader)
    {
        options.DebugBeginRestoreStorage();

        var fileGroups = GetFilesToProcess(dataReader).GroupBy(file => file.Module).ToList();
        var groupsProcessed = 0;
        foreach (var group in fileGroups)
        {
            foreach (var file in group)
            {
                var storage = await StorageFactory.GetStorageAsync(Dump ? file.Tenant : _columnMapper.GetTenantMapping(), group.Key);
                var quotaController = storage.QuotaController;
                storage.SetQuotaController(null);

                try
                {
                    var adjustedPath = file.Path;
                    var module = ModuleProvider.GetByStorageModule(file.Module, file.Domain);
                    if (module == null || module.TryAdjustFilePath(Dump, _columnMapper, ref adjustedPath))
                    {
                        var key = file.GetZipKey();
                        if (Dump)
                        {
                            key = CrossPlatform.PathCombine(KeyHelper.GetStorage(), key);
                        }

                        await using var stream = dataReader.GetEntry(key);
                        try
                        {
                            await storage.SaveAsync(file.Domain, adjustedPath, module != null ? module.PrepareData(key, stream, _columnMapper) : stream);
                        }
                        catch (Exception error)
                        {
                            options.WarningCantRestoreFile(file.Module, file.Path, error);
                        }
                    }
                }
                finally
                {
                    if (quotaController != null)
                    {
                        storage.SetQuotaController(quotaController);
                    }
                }
            }

            await SetCurrentStepProgress((int)(++groupsProcessed * 100 / (double)fileGroups.Count));
        }

        if (fileGroups.Count == 0)
        {
            await SetStepCompleted();
        }

        options.DebugEndRestoreStorage();
    }

    private async Task DoDeleteStorageAsync(IEnumerable<string> storageModules, IEnumerable<Tenant> tenants)
    {
        options.DebugBeginDeleteStorage();

        foreach (var tenant in tenants)
        {
            foreach (var module in storageModules)
            {
                var storage = await StorageFactory.GetStorageAsync(tenant.Id, module, _region);
                var domains = StorageFactoryConfig.GetDomainList(module, region:_region).ToList();

                domains.Add(string.Empty); //instead storage.DeleteFiles("\\", "*.*", true);

                foreach (var domain in domains)
                {
                    await ActionInvoker.TryAsync(
                        async state =>
                        {
                            if (await storage.IsDirectoryAsync((string)state))
                            {
                                await storage.DeleteFilesAsync((string)state, "\\", "*.*", true);
                            }
                        },
                        domain,
                        5,
                        onFailure: error => Logger.WarningCanNotDeleteFilesForDomain(domain, error)
                    );
                }

                await SetStepCompleted();
            }
        }

        Logger.DebugEndDeleteStorage();
    }

    private static List<BackupFileInfo> GetFilesToProcess(IDataReadOperator dataReader)
    {
        using var stream = dataReader.GetEntry(KeyHelper.GetStorageRestoreInfoZipKey());
        if (stream == null)
        {
            return [];
        }

        var restoreInfo = XElement.Load(new StreamReader(stream));

        return restoreInfo.Elements("file").Select(BackupFileInfo.FromXElement).ToList();
    }

    private void SetTenantActive(int tenantId)
    {
        using var connection = DbFactory.OpenConnection();
        var commandText = string.Format(
            "update tenants_tenants " +
            "set " +
            "  status={0}, " +
            "  last_modified='{1}', " +
            "  statuschanged='{1}' " +
            "where id = '{2}'",
            (int)TenantStatus.Active,
            DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            tenantId);

        var command = connection.CreateCommand().WithTimeout(120);
        command.CommandText = commandText;
        command.ExecuteNonQuery();
    }
}
