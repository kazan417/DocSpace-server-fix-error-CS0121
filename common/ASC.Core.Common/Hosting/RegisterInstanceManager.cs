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

namespace ASC.Core.Common.Hosting;

#nullable enable

public class RegisterInstanceManager<T>(
    IRegisterInstanceDao<T> registerInstanceRepository,
    IOptions<InstanceWorkerOptions<T>> optionsSettings) : IRegisterInstanceManager<T> where T : IHostedService
{
    private readonly InstanceWorkerOptions<T> _options = optionsSettings.Value;

    public async Task Register()
    {
        if (_options.SingletonMode)
        {
            return;
        }

        var workerTypeName = _options.WorkerTypeName;

        var instances = await registerInstanceRepository.GetAllAsync(workerTypeName);
        var registeredInstance = instances.FirstOrDefault(x => x.InstanceRegistrationId == _options.InstanceId);
        var instance = registeredInstance ?? new InstanceRegistration
        {
            InstanceRegistrationId = _options.InstanceId,
            WorkerTypeName = workerTypeName
        };

        instance.LastUpdated = DateTime.UtcNow;

        if (instances.Count != 0 && !instances.Any(x => x.IsActive))
        {
            var firstAliceInstance = FirstAliveInstance(instances);

            if (firstAliceInstance == null || firstAliceInstance.InstanceRegistrationId == instance.InstanceRegistrationId)
            {
                instance.IsActive = true;
            }
        }

        await registerInstanceRepository.AddOrUpdateAsync(instance);
        
        var oldRegistrations = instances.Where(IsOrphanInstance).ToList();

        foreach (var instanceRegistration in oldRegistrations)
        {
            await registerInstanceRepository.DeleteAsync(instanceRegistration.InstanceRegistrationId);
        }
    }

    public async Task UnRegister()
    {
        await registerInstanceRepository.DeleteAsync(_options.InstanceId);
    }

    public async Task<bool> IsActive()
    {
        if (_options.SingletonMode)
        {
            return true;
        }

        var workerTypeName = _options.WorkerTypeName;
        var instances = await registerInstanceRepository.GetAllAsync(workerTypeName);
        var instance = instances.FirstOrDefault(x => x.InstanceRegistrationId == _options.InstanceId);

        return instance is not null && instance.IsActive;
    }    

    private InstanceRegistration? FirstAliveInstance(IEnumerable<InstanceRegistration> instances)
    {
        Func<InstanceRegistration, long> getTicksCreationService = x => Convert.ToInt64(x.InstanceRegistrationId.Split('_').Last());

        return instances.Where(x => !IsOrphanInstance(x)).MinBy(getTicksCreationService);
    }

    private bool IsOrphanInstance(InstanceRegistration obj)
    {
        return obj.LastUpdated.HasValue && obj.LastUpdated.Value.AddSeconds(_options.TimeUntilUnregisterInSeconds) < DateTime.UtcNow;
    }
}