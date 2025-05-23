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

namespace ASC.Core.Tenants;

public class TenantCookieSettings : ISettings<TenantCookieSettings>
{
    public int Index { get; set; }
    public int LifeTime { get; set; }
    public bool Enabled { get; set; }

    public TenantCookieSettings GetDefault()
    {
        return GetInstance();
    }
    
    public DateTime LastModified { get; set; }

    public bool IsDefault()
    {
        var defaultSettings = GetInstance();

        return LifeTime == defaultSettings.LifeTime && Enabled == defaultSettings.Enabled;
    }

    public static TenantCookieSettings GetInstance()
    {
        return new TenantCookieSettings
        {
            LifeTime = 1440
        };
    }

    [JsonIgnore]
    public Guid ID => new("{16FB8E67-E96D-4B22-B217-C80F25C5DE1B}");
}

[Singleton]
public class TenantCookieSettingsConfig(IConfiguration configuration)
{
    public bool IsVisibleSettings { get; } = !(configuration.GetSection("web:hide-settings").Get<string[]>() ?? [])
                    .Contains("CookieSettings", StringComparer.CurrentCultureIgnoreCase);
}

[Scope]
public class TenantCookieSettingsHelper(TenantCookieSettingsConfig configuration, SettingsManager settingsManager)
{
    public async Task<TenantCookieSettings> GetForTenantAsync(int tenantId)
    {
        return configuration.IsVisibleSettings
                   ? await settingsManager.LoadAsync<TenantCookieSettings>(tenantId)
                   : TenantCookieSettings.GetInstance();
    }

    public async Task SetForTenantAsync(int tenantId, TenantCookieSettings settings = null)
    {
        if (!configuration.IsVisibleSettings)
        {
            return;
        }

        await settingsManager.SaveAsync(settings ?? TenantCookieSettings.GetInstance(), tenantId);
    }

    public async Task<TenantCookieSettings> GetForUserAsync(Guid userId)
    {
        return configuration.IsVisibleSettings
                   ? await settingsManager.LoadAsync<TenantCookieSettings>(userId)
                   : TenantCookieSettings.GetInstance();
    }

    public async Task<TenantCookieSettings> GetForUserAsync(int tenantId, Guid userId)
    {
        return configuration.IsVisibleSettings
                   ? await settingsManager.LoadAsync<TenantCookieSettings>(tenantId, userId)
                   : TenantCookieSettings.GetInstance();
    }

    public async Task SetForUserAsync(Guid userId, TenantCookieSettings settings = null)
    {
        if (!configuration.IsVisibleSettings)
        {
            return;
        }

        await settingsManager.SaveAsync(settings ?? TenantCookieSettings.GetInstance(), userId);
    }

    public async Task<DateTime> GetExpiresTimeAsync(int tenantId)
    {
        var settingsTenant = await GetForTenantAsync(tenantId);

        DateTime expires;

        if (settingsTenant.IsDefault() || !settingsTenant.Enabled)
        {
            expires = DateTime.UtcNow.AddYears(1);
        }
        else
        {
            expires = settingsTenant.LifeTime == 0 ? DateTime.MaxValue : DateTime.UtcNow.AddMinutes(settingsTenant.LifeTime);
        }

        return expires;
    }
}
