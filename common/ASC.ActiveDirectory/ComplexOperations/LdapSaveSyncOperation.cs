﻿// (c) Copyright Ascensio System SIA 2009-2025
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

namespace ASC.ActiveDirectory.ComplexOperations;

[Singleton]
public class LdapSaveSyncOperation(IServiceProvider serviceProvider, IDistributedTaskQueueFactory queueFactory)
{
    private readonly DistributedTaskQueue<LdapOperationJob> _progressQueue = queueFactory.CreateQueue<LdapOperationJob>();

    public async Task RunJobAsync(LdapSettings settings, Tenant tenant, LdapOperationType operationType, LdapLocalization resource = null, string userId = null)
    {
        var item = (await _progressQueue.GetAllTasks()).FirstOrDefault(t => t.TenantId == tenant.Id);
        if (item is { IsCompleted: true })
        {
            await _progressQueue.DequeueTask(item.Id);
            item = null;
        }
        if (item == null)
        {
            item = serviceProvider.GetRequiredService<LdapOperationJob>();
            await item.InitJobAsync(settings, tenant, operationType, resource, userId);
            await _progressQueue.EnqueueTask(item);
        }

        await item.PublishChanges();
    }

    public async Task<LdapOperationStatus> TestLdapSaveAsync(LdapSettings ldapSettings, Tenant tenant, string userId)
    {
        var (hasStarted, operations) = await HasStartedForTenant(tenant.Id, LdapOperationType.SyncTest, LdapOperationType.SaveTest);

        if (hasStarted)
        {
            return await ToLdapOperationStatus(tenant.Id);
        }

        if (operations.Any(o => o.Status <= DistributedTaskStatus.Running))
        {
            return GetStartProcessError();
        }

        var ldapLocalization = new LdapLocalization();
        ldapLocalization.Init(Resource.ResourceManager);

        await RunJobAsync(ldapSettings, tenant, LdapOperationType.SaveTest, ldapLocalization, userId);
        return await ToLdapOperationStatus(tenant.Id);
    }

    public async Task<LdapOperationStatus> SaveLdapSettingsAsync(LdapSettings ldapSettings, Tenant tenant, string userId)
    {
        var operations = await GetOperationsForTenant(tenant.Id);

        if (operations.Any(o => o.Status <= DistributedTaskStatus.Running))
        {
            return GetStartProcessError();
        }

        //ToDo
        ldapSettings.AccessRights.Clear();

        if (!ldapSettings.LdapMapping.ContainsKey(LdapSettings.MappingFields.MailAttribute) || string.IsNullOrEmpty(ldapSettings.LdapMapping[LdapSettings.MappingFields.MailAttribute]))
        {
            ldapSettings.SendWelcomeEmail = false;
            ldapSettings.DisableEmailVerification = false;
        }

        var ldapLocalization = new LdapLocalization();
        ldapLocalization.Init(Resource.ResourceManager, WebstudioNotifyPatternResource.ResourceManager);

        await RunJobAsync(ldapSettings, tenant, LdapOperationType.Save, ldapLocalization, userId);
        return await ToLdapOperationStatus(tenant.Id);
    }

    public async Task<LdapOperationStatus> SyncLdapAsync(LdapSettings ldapSettings, Tenant tenant, string userId)
    {
        var (hasStarted, operations) = await HasStartedForTenant(tenant.Id, LdapOperationType.Sync, LdapOperationType.Save);

        if (hasStarted)
        {
            return await ToLdapOperationStatus(tenant.Id);
        }

        if (operations.Any(o => o.Status <= DistributedTaskStatus.Running))
        {
            return GetStartProcessError();
        }

        var ldapLocalization = new LdapLocalization();
        ldapLocalization.Init(Resource.ResourceManager);

        await RunJobAsync(ldapSettings, tenant, LdapOperationType.Sync, ldapLocalization, userId);
        return await ToLdapOperationStatus(tenant.Id);
    }

    public async Task<LdapOperationStatus> TestLdapSyncAsync(LdapSettings ldapSettings, Tenant tenant)
    {
        var (hasStarted, operations) = await HasStartedForTenant(tenant.Id, LdapOperationType.SyncTest, LdapOperationType.SaveTest);

        if (hasStarted)
        {
            return await ToLdapOperationStatus(tenant.Id);
        }

        if (operations.Any(o => o.Status <= DistributedTaskStatus.Running))
        {
            return GetStartProcessError();
        }

        var ldapLocalization = new LdapLocalization();
        ldapLocalization.Init(Resource.ResourceManager);

        await RunJobAsync(ldapSettings, tenant, LdapOperationType.SyncTest, ldapLocalization);
        return await ToLdapOperationStatus(tenant.Id);
    }

    public async Task<LdapOperationStatus> ToLdapOperationStatus(int tenantId)
    {
        var operations = (await _progressQueue.GetAllTasks()).ToList();

        foreach (var o in operations)
        {
            if (Process.GetProcesses().Any(p => p.Id == o.InstanceId))
            {
                continue;
            }

            o.Percentage = 100;
            await _progressQueue.DequeueTask(o.Id);
        }

        var operation = operations.FirstOrDefault(t => t.TenantId == tenantId);

        if (operation == null)
        {
            return null;
        }

        if (DistributedTaskStatus.Running < operation.Status)
        {
            operation.Percentage = 100;
            await _progressQueue.DequeueTask(operation.Id);
        }

        var result = new LdapOperationStatus
        {
            Id = operation.Id,
            Completed = operation.Finished,
            Percents = (int)operation.Percentage,
            Status = operation.Result,
            Error = operation.Error,
            CertificateConfirmRequest = operation.CertRequest != "" ? operation.CertRequest : null,
            Source = operation.Source,
            OperationType = operation.OperationType.ToString(),
            Warning = operation.Warning
        };

        if (!(string.IsNullOrEmpty(result.Warning)))
        {
            operation.Warning = ""; // "mark" as read
        }

        return result;
    }

    private static LdapOperationStatus GetStartProcessError()
    {
        var result = new LdapOperationStatus
        {
            Id = null,
            Completed = true,
            Percents = 0,
            Status = "",
            Error = Resource.LdapSettingsTooManyOperations,
            CertificateConfirmRequest = null,
            Source = ""
        };

        return result;
    }

    private async Task<(bool hasStarted, List<LdapOperationJob> operations)> HasStartedForTenant(int tenantId, LdapOperationType arg1, LdapOperationType arg2)
    {
        var operations = await GetOperationsForTenant(tenantId);

        var hasStarted = operations.Any(o =>
        {
            var opType = o.OperationType;

            return o.Status <= DistributedTaskStatus.Running && (opType == arg1 || opType == arg2);
        });

        return (hasStarted, operations);
    }

    private async Task<List<LdapOperationJob>> GetOperationsForTenant(int tenantId)
    {
        return (await _progressQueue.GetAllTasks())
            .Where(t => t.TenantId == tenantId)
            .ToList();
    }
}