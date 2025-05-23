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

namespace ASC.Data.Storage.Configuration;

[Singleton]
public class BaseStorageSettingsListener(IServiceProvider serviceProvider, ICacheNotify<ConsumerCacheItem> cacheNotify)
{
    private readonly Lock _locker = new();
    private volatile bool _subscribed;

    public void Subscribe()
    {
        if (_subscribed)
        {
            return;
        }

        lock (_locker)
        {
            if (_subscribed)
            {
                return;
            }

            _subscribed = true;

            cacheNotify.Subscribe(async i =>
            {
                using var scope = serviceProvider.CreateScope();

                var scopeClass = scope.ServiceProvider.GetService<BaseStorageSettingsListenerScope>();
                var (storageSettingsHelper, settingsManager) = scopeClass;
                var settings = await settingsManager.LoadAsync<StorageSettings>(i.TenantId);
                if (i.Name == settings.Module)
                {
                    await storageSettingsHelper.ClearAsync(settings);
                }

                var cdnSettings = await settingsManager.LoadAsync<CdnStorageSettings>(i.TenantId);
                if (i.Name == cdnSettings.Module)
                {
                    await storageSettingsHelper.ClearAsync(cdnSettings);
                }
            }, CacheNotifyAction.Remove);
        }
    }
}

/// <summary>
/// The base storage settings.
/// </summary>
public abstract class BaseStorageSettings<T> : ISettings<BaseStorageSettings<T>> where T : class, ISettings<T>, new()
{
    /// <summary>
    /// The storage name.
    /// </summary>
    public string Module { get; set; }

    /// <summary>
    /// The storage properties.
    /// </summary>
    public Dictionary<string, string> Props { get; set; }
    
    [JsonIgnore]
    public virtual Func<DataStoreConsumer, DataStoreConsumer> Switch => d => d;
    public abstract Guid ID { get; }
    
    internal ICacheNotify<DataStoreCacheItem> Cache { get; set; }

    public BaseStorageSettings<T> GetDefault()
    {
        throw new NotImplementedException();
    }
    
    public DateTime LastModified { get; set; }
}

/// <summary>
/// The storage settings.
/// </summary>
public class StorageSettings : BaseStorageSettings<StorageSettings>, ISettings<StorageSettings>
{
    /// <summary>
    /// The storage ID.
    /// </summary>
    [JsonIgnore]
    public override Guid ID => new("F13EAF2D-FA53-44F1-A6D6-A5AEDA46FA2B");

    StorageSettings ISettings<StorageSettings>.GetDefault()
    {
        return new StorageSettings();
    }
}

/// <summary>
/// The CDN storage settings.
/// </summary>
[Scope]
public class CdnStorageSettings : BaseStorageSettings<CdnStorageSettings>, ISettings<CdnStorageSettings>
{
    /// <summary>
    /// The CDN storage ID.
    /// </summary>
    [JsonIgnore]
    public override Guid ID => new("0E9AE034-F398-42FE-B5EE-F86D954E9FB2");

    [JsonIgnore]
    public override Func<DataStoreConsumer, DataStoreConsumer> Switch => d => d.Cdn;

    CdnStorageSettings ISettings<CdnStorageSettings>.GetDefault()
    {
        return new CdnStorageSettings();
    }
}

[Scope]
public class StorageSettingsHelper
{
    private readonly StorageFactoryConfig _storageFactoryConfig;
    private readonly ICacheNotify<DataStoreCacheItem> _cache;
    private readonly TenantManager _tenantManager;
    private readonly SettingsManager _settingsManager;
    private readonly ConsumerFactory _consumerFactory;
    private readonly IServiceProvider _serviceProvider;
    private IDataStore _dataStore;

    public StorageSettingsHelper(
        BaseStorageSettingsListener baseStorageSettingsListener,
        StorageFactoryConfig storageFactoryConfig,
        ICacheNotify<DataStoreCacheItem> cache,
        TenantManager tenantManager,
        SettingsManager settingsManager,
        ConsumerFactory consumerFactory,
        IServiceProvider serviceProvider)
    {
        baseStorageSettingsListener.Subscribe();
        _storageFactoryConfig = storageFactoryConfig;
        _cache = cache;
        _tenantManager = tenantManager;
        _settingsManager = settingsManager;
        _consumerFactory = consumerFactory;
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> SaveAsync<T>(BaseStorageSettings<T> baseStorageSettings) where T : class, ISettings<T>, new()
    {
        await ClearDataStoreCacheAsync();

        return await _settingsManager.SaveAsync(baseStorageSettings);
    }

    public async Task ClearAsync<T>(BaseStorageSettings<T> baseStorageSettings) where T : class, ISettings<T>, new()
    {
        baseStorageSettings.Module = null;
        baseStorageSettings.Props = null;
        await SaveAsync(baseStorageSettings);
    }

    public async Task<DataStoreConsumer> DataStoreConsumerAsync<T>(BaseStorageSettings<T> baseStorageSettings) where T : class, ISettings<T>, new()
    {
        if (string.IsNullOrEmpty(baseStorageSettings.Module) || baseStorageSettings.Props == null)
        {
            return new DataStoreConsumer();
        }

        var consumer = _consumerFactory.GetByKey<DataStoreConsumer>(baseStorageSettings.Module);

        if (!await consumer.GetIsSetAsync())
        {
            return new DataStoreConsumer();
        }

        var dataStoreConsumer = (DataStoreConsumer)consumer.Clone();

        foreach (var prop in baseStorageSettings.Props)
        {
            await dataStoreConsumer.SetAsync(prop.Key, prop.Value);
        }

        return dataStoreConsumer;
    }

    public async Task<IDataStore> DataStoreAsync<T>(BaseStorageSettings<T> baseStorageSettings) where T : class, ISettings<T>, new()
    {
        if (_dataStore != null)
        {
            return _dataStore;
        }

        var dataStoreConsumer = await DataStoreConsumerAsync(baseStorageSettings);
        var handlerType = dataStoreConsumer.HandlerType;
        if (handlerType == null)
        {
            return null;
        }

        return _dataStore = await ((IDataStore)_serviceProvider.GetService(handlerType))
            .ConfigureAsync((_tenantManager.GetCurrentTenantId()).ToString(), null, null, dataStoreConsumer, null);
    }

    internal async Task ClearDataStoreCacheAsync()
    {
        var path = TenantPath.CreatePath(_tenantManager.GetCurrentTenantId());

        foreach (var module in _storageFactoryConfig.GetModuleList("", true))
        {
            await _cache.PublishAsync(new DataStoreCacheItem { TenantId = path, Module = module }, CacheNotifyAction.Remove);
        }
    }
}

[Scope]
public record BaseStorageSettingsListenerScope(
    StorageSettingsHelper StorageSettingsHelper,
    SettingsManager SettingsManager);