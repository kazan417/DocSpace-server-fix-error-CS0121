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

namespace ASC.Core.Caching;

[Singleton]
class QuotaServiceCache
{
    internal const string KeyQuota = "quota";
    internal const string KeyQuotaRows = "quotarows";
    internal const string KeyUserQuotaRows = "userquotarows";
    internal readonly ICache Cache;
    internal readonly ICacheNotify<QuotaCacheItem> CacheNotify;
    internal readonly bool QuotaCacheEnabled;

    public QuotaServiceCache(IConfiguration Configuration, ICacheNotify<QuotaCacheItem> cacheNotify, ICache cache)
    {
        if (Configuration["core:enable-quota-cache"] == null)
        {
            QuotaCacheEnabled = true;
        }
        else
        {
            QuotaCacheEnabled = !bool.TryParse(Configuration["core:enable-quota-cache"], out var enabled) || enabled;
        }

        CacheNotify = cacheNotify;
        Cache = cache;

        cacheNotify.Subscribe(i => Cache.Remove(i.Key == KeyQuota ? KeyQuota : i.Key), CacheNotifyAction.Any);
    }
}


[Scope(typeof(IQuotaService))]
class CachedQuotaService() : IQuotaService
{
    private readonly DbQuotaService _service;
    private readonly ICache _cache;
    private readonly ICacheNotify<QuotaCacheItem> _cacheNotify;
    private readonly QuotaServiceCache _quotaServiceCache;

    private readonly GeolocationHelper _geolocationHelper;

    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

    public CachedQuotaService(DbQuotaService service, QuotaServiceCache quotaServiceCache, GeolocationHelper geolocationHelper) : this()
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _quotaServiceCache = quotaServiceCache;
        _cache = quotaServiceCache.Cache;
        _cacheNotify = quotaServiceCache.CacheNotify;
        _geolocationHelper = geolocationHelper;
    }

    public async Task<IEnumerable<TenantQuota>> GetTenantQuotasAsync()
    {
        var cacheKey = QuotaServiceCache.KeyQuota + (await _geolocationHelper.GetIPGeolocationFromHttpContextAsync()).Key;
        var quotas = _cache.Get<IEnumerable<TenantQuota>>(cacheKey);
        if (quotas == null)
        {
            quotas = await _service.GetTenantQuotasAsync();
            if (_quotaServiceCache.QuotaCacheEnabled)
            {
                _cache.Insert(cacheKey, quotas, DateTime.UtcNow.Add(_cacheExpiration));
            }
        }

        return quotas;
    }

    public async Task<TenantQuota> GetTenantQuotaAsync(int id)
    {
        return (await GetTenantQuotasAsync()).SingleOrDefault(q => q.TenantId == id);
    }

    public async Task<TenantQuota> SaveTenantQuotaAsync(TenantQuota quota)
    {
        var q = await _service.SaveTenantQuotaAsync(quota);
        await _cacheNotify.PublishAsync(new QuotaCacheItem { Key = QuotaServiceCache.KeyQuota }, CacheNotifyAction.Any);

        return q;
    }

    public Task RemoveTenantQuotaAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task SetTenantQuotaRowAsync(TenantQuotaRow row, bool exchange)
    {
        await _service.SetTenantQuotaRowAsync(row, exchange);
        await _cacheNotify.PublishAsync(new QuotaCacheItem { Key = GetKey(row.TenantId) }, CacheNotifyAction.Any);

        if (row.UserId != Guid.Empty)
        {
            await _cacheNotify.PublishAsync(new QuotaCacheItem { Key = GetKey(row.TenantId, row.UserId) }, CacheNotifyAction.Any);
        }
    }

    public async Task<IEnumerable<TenantQuotaRow>> FindTenantQuotaRowsAsync(int tenantId)
    {
        var key = GetKey(tenantId);
        var result = _cache.Get<IEnumerable<TenantQuotaRow>>(key);

        if (result == null)
        {
            result = await _service.FindTenantQuotaRowsAsync(tenantId);
            _cache.Insert(key, result, DateTime.UtcNow.Add(_cacheExpiration));
        }

        return result;
    }

    public async Task<IEnumerable<TenantQuotaRow>> FindUserQuotaRowsAsync(int tenantId, Guid userId)
    {
        var key = GetKey(tenantId, userId);
        var result = _cache.Get<IEnumerable<TenantQuotaRow>>(key);

        if (result == null)
        {
            result = await _service.FindUserQuotaRowsAsync(tenantId, userId);
            _cache.Insert(key, result, DateTime.UtcNow.Add(_cacheExpiration));
        }

        return result;
    }

    public string GetKey(int tenant)
    {
        return QuotaServiceCache.KeyQuotaRows + tenant;
    }

    public string GetKey(int tenant, Guid userId)
    {
        return QuotaServiceCache.KeyQuotaRows + tenant + userId;
    }
}
