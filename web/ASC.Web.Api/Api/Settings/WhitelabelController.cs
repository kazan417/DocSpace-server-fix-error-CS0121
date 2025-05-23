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

namespace ASC.Web.Api.Controllers.Settings;

public class WhitelabelController(
    ApiContext apiContext,
    PermissionContext permissionContext,
    SettingsManager settingsManager,
    WebItemManager webItemManager,
    TenantInfoSettingsHelper tenantInfoSettingsHelper,
    TenantWhiteLabelSettingsHelper tenantWhiteLabelSettingsHelper,
    TenantLogoManager tenantLogoManager,
    CoreBaseSettings coreBaseSettings,
    CommonLinkUtility commonLinkUtility,
    IFusionCache fusionCache,
    IHttpContextAccessor httpContextAccessor,
    IMapper mapper,
    CompanyWhiteLabelSettingsHelper companyWhiteLabelSettingsHelper,
    TenantManager tenantManager,
    TenantExtra tenantExtra,
    StorageFactory storageFactory)
    : BaseSettingsController(apiContext, fusionCache, webItemManager, httpContextAccessor)
{
    #region Logos

    /// <summary>
    /// Saves the white label logos specified in the request.
    /// </summary>
    /// <short>
    /// Save the white label logos
    /// </short>
    /// <path>api/2.0/settings/whitelabel/logos/save</path>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Boolean value: true if the operation is sucessful", typeof(bool))]
    [SwaggerResponse(403, "No permissions to perform this action")]
    [HttpPost("whitelabel/logos/save")]
    public async Task<bool> SaveWhiteLabelSettingsAsync(WhiteLabelRequestsDto inDto, [FromQuery] WhiteLabelQueryRequestsDto inQueryDto)
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        if (inQueryDto is { IsDefault: not null } && inQueryDto.IsDefault.Value)
        {
            await DemandRebrandingPermissionAsync();

            await SaveWhiteLabelSettingsForDefaultTenantAsync(inDto);
        }
        else
        {
            await tenantLogoManager.DemandWhiteLabelPermissionAsync();

            await SaveWhiteLabelSettingsForCurrentTenantAsync(inDto);
        }

        return true;
    }

    private async Task SaveWhiteLabelSettingsForCurrentTenantAsync(WhiteLabelRequestsDto inDto)
    {
        var settings = await settingsManager.LoadAsync<TenantWhiteLabelSettings>();

        var tenant = tenantManager.GetCurrentTenant();

        await SaveWhiteLabelSettingsForTenantAsync(settings, null, tenant.Id, inDto);
    }

    private async Task SaveWhiteLabelSettingsForDefaultTenantAsync(WhiteLabelRequestsDto inDto)
    {
        var settings = await settingsManager.LoadForDefaultTenantAsync<TenantWhiteLabelSettings>();

        var storage = await storageFactory.GetStorageAsync(Tenant.DefaultTenant, "static_partnerdata");

        await SaveWhiteLabelSettingsForTenantAsync(settings, storage, Tenant.DefaultTenant, inDto);
    }

    private async Task SaveWhiteLabelSettingsForTenantAsync(TenantWhiteLabelSettings settings, IDataStore storage, int tenantId, WhiteLabelRequestsDto inDto)
    {
        if (inDto.Logo != null)
        {
            var logoDict = new Dictionary<int, KeyValuePair<string, string>>();

            foreach (var l in inDto.Logo)
            {
                var key = Int32.Parse(l.Key);

                logoDict.Add(key, new KeyValuePair<string, string>(l.Value.Light, l.Value.Dark));
            }

            await tenantWhiteLabelSettingsHelper.SetLogo(settings, logoDict, storage);
        }

        await settingsManager.SaveAsync(settings, tenantId);

        await tenantLogoManager.RemoveMailLogoDataFromCacheAsync();
    }

    /// <summary>
    /// Saves the white label logos from files.
    /// </summary>
    /// <short>
    /// Save the white label logos from files
    /// </short>
    /// <path>api/2.0/settings/whitelabel/logos/savefromfiles</path>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Boolean value: true if the operation is sucessful", typeof(bool))]
    [SwaggerResponse(403, "No permissions to perform this action")]
    [SwaggerResponse(409, "No input files")]
    [HttpPost("whitelabel/logos/savefromfiles")]
    public async Task<bool> SaveWhiteLabelSettingsFromFilesAsync([FromQuery] WhiteLabelQueryRequestsDto inQueryDto)
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        if (HttpContext.Request.Form.Files == null || HttpContext.Request.Form.Files.Count == 0)
        {
            throw new InvalidOperationException("No input files");
        }

        if (inQueryDto is { IsDefault: not null } && inQueryDto.IsDefault.Value)
        {
            await DemandRebrandingPermissionAsync();

            await SaveWhiteLabelSettingsFromFilesForDefaultTenantAsync();
        }
        else
        {
            await tenantLogoManager.DemandWhiteLabelPermissionAsync();

            await SaveWhiteLabelSettingsFromFilesForCurrentTenantAsync();
        }

        return true;
    }

    private async Task SaveWhiteLabelSettingsFromFilesForCurrentTenantAsync()
    {
        var settings = await settingsManager.LoadAsync<TenantWhiteLabelSettings>();

        var tenant = tenantManager.GetCurrentTenant();

        await SaveWhiteLabelSettingsFromFilesForTenantAsync(settings, null, tenant.Id);
    }

    private async Task SaveWhiteLabelSettingsFromFilesForDefaultTenantAsync()
    {
        var settings = await settingsManager.LoadForDefaultTenantAsync<TenantWhiteLabelSettings>();

        var storage = await storageFactory.GetStorageAsync(Tenant.DefaultTenant, "static_partnerdata");

        await SaveWhiteLabelSettingsFromFilesForTenantAsync(settings, storage, Tenant.DefaultTenant);
    }

    private async Task SaveWhiteLabelSettingsFromFilesForTenantAsync(TenantWhiteLabelSettings settings, IDataStore storage, int tenantId)
    {
        foreach (var f in HttpContext.Request.Form.Files)
        {
            if (f.FileName.Contains("dark"))
            {
                GetParts(f.FileName, out var logoType, out var fileExt);

                await tenantWhiteLabelSettingsHelper.SetLogoFromStream(settings, logoType, fileExt, f.OpenReadStream(), true, storage);
            }
            else
            {
                GetParts(f.FileName, out var logoType, out var fileExt);

                await tenantWhiteLabelSettingsHelper.SetLogoFromStream(settings, logoType, fileExt, f.OpenReadStream(), false, storage);
            }
        }

        await settingsManager.SaveAsync(settings, tenantId);

        await tenantLogoManager.RemoveMailLogoDataFromCacheAsync();
    }

    private void GetParts(string fileName, out WhiteLabelLogoType logoType, out string fileExt)
    {
        var parts = fileName.Split('.');
        logoType = (WhiteLabelLogoType)Convert.ToInt32(parts[0]);
        fileExt = parts[^1];
    }

    /// <summary>
    /// Returns the white label logos.
    /// </summary>
    /// <short>
    /// Get the white label logos
    /// </short>
    /// <path>api/2.0/settings/whitelabel/logos</path>
    /// <requiresAuthorization>false</requiresAuthorization>
    /// <collection>list</collection>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "White label logos", typeof(IAsyncEnumerable<WhiteLabelItemDto>))]
    [AllowNotPayment, AllowAnonymous, AllowSuspended]
    [HttpGet("whitelabel/logos")]
    public async IAsyncEnumerable<WhiteLabelItemDto> GetWhiteLabelLogosAsync([FromQuery] WhiteLabelQueryRequestsDto inQueryDto)
    {
        var isDefault = inQueryDto is { IsDefault: not null } && inQueryDto.IsDefault.Value;

        var tenantWhiteLabelSettings = isDefault ? null : await settingsManager.LoadAsync<TenantWhiteLabelSettings>();

        foreach (var logoType in Enum.GetValues<WhiteLabelLogoType>())
        {
            if (logoType == WhiteLabelLogoType.Notification)
            {
                continue;
            }

            var result = new WhiteLabelItemDto
            {
                Name = logoType.ToStringFast(),
                Size = TenantWhiteLabelSettings.GetSize(logoType)
            };

            if (inQueryDto is { IsDark: not null })
            {
                var path = commonLinkUtility.GetFullAbsolutePath(isDefault
                    ? await tenantWhiteLabelSettingsHelper.GetAbsoluteDefaultLogoPathAsync(logoType, inQueryDto.IsDark.Value)
                    : await tenantWhiteLabelSettingsHelper.GetAbsoluteLogoPathAsync(tenantWhiteLabelSettings, logoType, inQueryDto.IsDark.Value));

                if (inQueryDto.IsDark.Value)
                {
                    result.Path = new WhiteLabelItemPathDto
                    {
                        Dark = path
                    };
                }
                else
                {
                    result.Path = new WhiteLabelItemPathDto
                    {
                        Light = path
                    };
                }
            }
            else
            {
                var lightPath = commonLinkUtility.GetFullAbsolutePath(isDefault
                    ? await tenantWhiteLabelSettingsHelper.GetAbsoluteDefaultLogoPathAsync(logoType, false)
                    : await tenantWhiteLabelSettingsHelper.GetAbsoluteLogoPathAsync(tenantWhiteLabelSettings, logoType));

                var darkPath = commonLinkUtility.GetFullAbsolutePath(isDefault
                    ? await tenantWhiteLabelSettingsHelper.GetAbsoluteDefaultLogoPathAsync(logoType, true)
                    : await tenantWhiteLabelSettingsHelper.GetAbsoluteLogoPathAsync(tenantWhiteLabelSettings, logoType, true));

                if (lightPath == darkPath)
                {
                    darkPath = null;
                }

                result.Path = new WhiteLabelItemPathDto
                {
                    Light = lightPath,
                    Dark = darkPath
                };
            }

            yield return result;
        }
    }

    /// <summary>
    /// Specifies if the white label logos are default or not.
    /// </summary>
    /// <short>
    /// Check the default white label logos
    /// </short>
    /// <path>api/2.0/settings/whitelabel/logos/isdefault</path>
    /// <collection>list</collection>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Request properties of white label logos", typeof(IAsyncEnumerable<IsDefaultWhiteLabelLogosDto>))]
    [HttpGet("whitelabel/logos/isdefault")]
    public async IAsyncEnumerable<IsDefaultWhiteLabelLogosDto> GetIsDefaultWhiteLabelLogos([FromQuery] WhiteLabelQueryRequestsDto inQueryDto)
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        var tenantWhiteLabelSettings = inQueryDto is { IsDefault: not null } && inQueryDto.IsDefault.Value
            ? await settingsManager.LoadForDefaultTenantAsync<TenantWhiteLabelSettings>()
            : await settingsManager.LoadAsync<TenantWhiteLabelSettings>();

        foreach (var logoType in Enum.GetValues<WhiteLabelLogoType>())
        {
            var result = new IsDefaultWhiteLabelLogosDto
            {
                Name = logoType.ToStringFast(),
                Default = tenantWhiteLabelSettings.GetIsDefault(logoType)
            };

            yield return result;
        }
    }

    /// <summary>
    /// Restores the white label logos.
    /// </summary>
    /// <short>
    /// Restore the white label logos
    /// </short>
    /// <path>api/2.0/settings/whitelabel/logos/restore</path>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Boolean value: true if the operation is successful", typeof(bool))]
    [SwaggerResponse(403, "No permissions to perform this action")]
    [HttpPut("whitelabel/logos/restore")]
    public async Task<bool> RestoreWhiteLabelLogosAsync([FromQuery] WhiteLabelQueryRequestsDto inQueryDto)
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        if (inQueryDto is { IsDefault: not null } && inQueryDto.IsDefault.Value)
        {
            await DemandRebrandingPermissionAsync(false);

            await RestoreWhiteLabelLogosForDefaultTenantAsync();
        }
        else
        {
            await RestoreWhiteLabelLogosForCurrentTenantAsync();
        }

        return true;
    }

    private async Task RestoreWhiteLabelLogosForCurrentTenantAsync()
    {
        var settings = await settingsManager.LoadAsync<TenantWhiteLabelSettings>();
        var tenant = tenantManager.GetCurrentTenant();

        await RestoreWhiteLabelLogosForTenantAsync(settings, null, tenant.Id);

        var tenantInfoSettings = await settingsManager.LoadAsync<TenantInfoSettings>();
        await tenantInfoSettingsHelper.RestoreDefaultLogoAsync(tenantInfoSettings, tenantLogoManager);
        await settingsManager.SaveAsync(tenantInfoSettings);
    }

    private async Task RestoreWhiteLabelLogosForDefaultTenantAsync()
    {
        var settings = await settingsManager.LoadForDefaultTenantAsync<TenantWhiteLabelSettings>();
        var storage = await storageFactory.GetStorageAsync(Tenant.DefaultTenant, "static_partnerdata");

        await RestoreWhiteLabelLogosForTenantAsync(settings, storage, Tenant.DefaultTenant);
    }

    private async Task RestoreWhiteLabelLogosForTenantAsync(TenantWhiteLabelSettings settings, IDataStore storage, int tenantId)
    {
        await tenantWhiteLabelSettingsHelper.RestoreDefaultLogos(settings, tenantLogoManager, tenantId, storage);
    }

    #endregion

    #region Logo Text

    /// <summary>
    /// Saves the white label logo text specified in the request.
    /// </summary>
    /// <short>
    /// Save the white label logo text settings
    /// </short>
    /// <path>api/2.0/settings/whitelabel/logotext/save</path>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Boolean value: true if the operation is sucessful", typeof(bool))]
    [SwaggerResponse(403, "No permissions to perform this action")]
    [HttpPost("whitelabel/logotext/save")]
    public async Task<bool> SaveWhiteLabelLogoTextAsync(WhiteLabelRequestsDto inDto, [FromQuery] WhiteLabelQueryRequestsDto inQueryDto)
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        int tenantId;

        if (inQueryDto is { IsDefault: not null } && inQueryDto.IsDefault.Value)
        {
            await DemandRebrandingPermissionAsync();

            tenantId = Tenant.DefaultTenant;
        }
        else
        {
            await tenantLogoManager.DemandWhiteLabelPermissionAsync();

            tenantId = tenantManager.GetCurrentTenantId();
        }

        var settings = await settingsManager.LoadAsync<TenantWhiteLabelSettings>(tenantId);

        settings.SetLogoText(inDto.LogoText);

        await settingsManager.SaveAsync(settings, tenantId);

        return true;
    }

    /// <summary>
    /// Returns the white label logo text.
    /// </summary>
    /// <short>
    /// Get the white label logo text
    /// </short>
    /// <path>api/2.0/settings/whitelabel/logotext</path>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Logo text", typeof(string))]
    [AllowNotPayment]
    [HttpGet("whitelabel/logotext")]
    public async Task<string> GetWhiteLabelLogoTextAsync([FromQuery] WhiteLabelQueryRequestsDto inQueryDto)
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        var settings = inQueryDto is { IsDefault: not null } && inQueryDto.IsDefault.Value
            ? await settingsManager.LoadForDefaultTenantAsync<TenantWhiteLabelSettings>()
            : await settingsManager.LoadAsync<TenantWhiteLabelSettings>();

        return settings.LogoText ?? TenantWhiteLabelSettings.DefaultLogoText;
    }

    /// <summary>
    /// Specifies if the white label logo text are default or not.
    /// </summary>
    /// <short>
    /// Check the default white label logo text
    /// </short>
    /// <path>api/2.0/settings/whitelabel/logotext/isdefault</path>
    /// <collection>list</collection>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Request properties of white label logos", typeof(IsDefaultWhiteLabelLogosDto))]
    [HttpGet("whitelabel/logotext/isdefault")]
    public async Task<IsDefaultWhiteLabelLogosDto> GetIsDefaultWhiteLabelLogoText([FromQuery] WhiteLabelQueryRequestsDto inQueryDto)
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        var tenantWhiteLabelSettings = inQueryDto is { IsDefault: not null } && inQueryDto.IsDefault.Value
            ? await settingsManager.LoadForDefaultTenantAsync<TenantWhiteLabelSettings>()
            : await settingsManager.LoadAsync<TenantWhiteLabelSettings>();

        return new IsDefaultWhiteLabelLogosDto
        {
            Name = "logotext",
            Default = string.IsNullOrEmpty(tenantWhiteLabelSettings.LogoText)
        };
    }

    /// <summary>
    /// Restores the white label logo text.
    /// </summary>
    /// <short>
    /// Restore the white label logo text
    /// </short>
    /// <path>api/2.0/settings/whitelabel/logotext/restore</path>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Boolean value: true if the operation is successful", typeof(bool))]
    [SwaggerResponse(403, "No permissions to perform this action")]
    [HttpPut("whitelabel/logotext/restore")]
    public async Task<bool> RestoreWhiteLabelLogoTextAsync([FromQuery] WhiteLabelQueryRequestsDto inQueryDto)
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        int tenantId;

        if (inQueryDto is { IsDefault: not null } && inQueryDto.IsDefault.Value)
        {
            await DemandRebrandingPermissionAsync(false);

            tenantId = Tenant.DefaultTenant;
        }
        else
        {
            tenantId = tenantManager.GetCurrentTenantId();
        }

        var settings = await settingsManager.LoadAsync<TenantWhiteLabelSettings>(tenantId);

        await tenantWhiteLabelSettingsHelper.RestoreDefaultLogoText(settings, tenantId);

        return true;
    }

    #endregion

    #region Company

    /// <summary>
    /// Returns the licensor data.
    /// </summary>
    /// <short>Get the licensor data</short>
    /// <path>api/2.0/settings/companywhitelabel</path>
    /// <collection>list</collection>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "List of company white label settings", typeof(List<CompanyWhiteLabelSettings>))]
    [HttpGet("companywhitelabel")]
    public async Task<List<CompanyWhiteLabelSettings>> GetLicensorDataAsync()
    {
        var result = new List<CompanyWhiteLabelSettings>();

        var instance = await companyWhiteLabelSettingsHelper.InstanceAsync();

        result.Add(instance);

        if (!companyWhiteLabelSettingsHelper.IsDefault(instance) && !instance.IsLicensor)
        {
            result.Add(settingsManager.GetDefault<CompanyWhiteLabelSettings>());
        }

        return result;
    }

    /// <summary>
    /// Saves the company white label settings specified in the request.
    /// </summary>
    /// <short>Save the company white label settings</short>
    /// <path>api/2.0/settings/rebranding/company</path>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Boolean value: true if the operation is successful", typeof(bool))]
    [SwaggerResponse(400, "Argument is empty or invalid")]
    [SwaggerResponse(403, "No permissions to perform this action")]
    [HttpPost("rebranding/company")]
    public async Task<bool> SaveCompanyWhiteLabelSettingsAsync(CompanyWhiteLabelSettingsWrapper wrapper)
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        await DemandRebrandingPermissionAsync();

        ArgumentNullException.ThrowIfNull(wrapper?.Settings, "settings");
        ArgumentNullException.ThrowIfNull(wrapper.Settings.Email, "email");
        ArgumentNullException.ThrowIfNull(wrapper.Settings.Site, "site");

        if (wrapper.Settings.Email.TestEmailPunyCode())
        {
            throw new ArgumentException("email");
        }

        if (wrapper.Settings.Site.TestUrlPunyCode())
        {
            throw new ArgumentException("site");
        }

        wrapper.Settings.IsLicensor = false;

        await settingsManager.SaveForDefaultTenantAsync(wrapper.Settings);

        return true;
    }

    /// <summary>
    /// Returns the company white label settings.
    /// </summary>
    /// <short>Get the company white label settings</short>
    /// <path>api/2.0/settings/rebranding/company</path>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Company white label settings", typeof(CompanyWhiteLabelSettingsDto))]
    [AllowNotPayment]
    [HttpGet("rebranding/company")]
    public async Task<CompanyWhiteLabelSettingsDto> GetCompanyWhiteLabelSettingsAsync()
    {
        var settings = await settingsManager.LoadForDefaultTenantAsync<CompanyWhiteLabelSettings>(HttpContext.GetIfModifiedSince());
        
        return HttpContext.TryGetFromCache(settings.LastModified) ? null :   mapper.Map<CompanyWhiteLabelSettings, CompanyWhiteLabelSettingsDto>(settings);
    }

    /// <summary>
    /// Deletes the company white label settings.
    /// </summary>
    /// <short>Delete the company white label settings</short>
    /// <path>api/2.0/settings/rebranding/company</path>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Default company white label settings", typeof(CompanyWhiteLabelSettings))]
    [SwaggerResponse(403, "No permissions to perform this action")]
    [HttpDelete("rebranding/company")]
    public async Task<CompanyWhiteLabelSettings> DeleteCompanyWhiteLabelSettingsAsync()
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        await DemandRebrandingPermissionAsync(false);

        var defaultSettings = settingsManager.GetDefault<CompanyWhiteLabelSettings>();

        await settingsManager.SaveForDefaultTenantAsync(defaultSettings);

        return defaultSettings;
    }

    #endregion

    #region Additional

    /// <summary>
    /// Saves the additional white label settings specified in the request.
    /// </summary>
    /// <short>Save the additional white label settings</short>
    /// <path>api/2.0/settings/rebranding/additional</path>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Boolean value: true if the operation is successful", typeof(bool))]
    [SwaggerResponse(400, "Settings is empty")]
    [SwaggerResponse(403, "No permissions to perform this action")]
    [HttpPost("rebranding/additional")]
    public async Task<bool> SaveAdditionalWhiteLabelSettingsAsync(AdditionalWhiteLabelSettingsWrapper wrapper)
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        await DemandRebrandingPermissionAsync();

        ArgumentNullException.ThrowIfNull(wrapper?.Settings, "settings");

        await settingsManager.SaveForDefaultTenantAsync(wrapper.Settings);

        return true;
    }

    /// <summary>
    /// Returns the additional white label settings.
    /// </summary>
    /// <short>Get the additional white label settings</short>
    /// <path>api/2.0/settings/rebranding/additional</path>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Additional white label settings", typeof(AdditionalWhiteLabelSettingsDto))]
    [AllowNotPayment]
    [HttpGet("rebranding/additional")]
    public async Task<AdditionalWhiteLabelSettingsDto> GetAdditionalWhiteLabelSettingsAsync()
    {
        var settings = await settingsManager.LoadForDefaultTenantAsync<AdditionalWhiteLabelSettings>();

        return mapper.Map<AdditionalWhiteLabelSettings, AdditionalWhiteLabelSettingsDto>(settings);
    }

    /// <summary>
    /// Deletes the additional white label settings.
    /// </summary>
    /// <short>Delete the additional white label settings</short>
    /// <path>api/2.0/settings/rebranding/additional</path>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Default additional white label settings", typeof(AdditionalWhiteLabelSettings))]
    [SwaggerResponse(403, "No permissions to perform this action")]
    [HttpDelete("rebranding/additional")]
    public async Task<AdditionalWhiteLabelSettings> DeleteAdditionalWhiteLabelSettingsAsync()
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        await DemandRebrandingPermissionAsync(false);

        var defaultSettings = settingsManager.GetDefault<AdditionalWhiteLabelSettings>();

        await settingsManager.SaveForDefaultTenantAsync(defaultSettings);

        return defaultSettings;
    }

    #endregion

    #region Mail

    /// <summary>
    /// Saves the mail white label settings specified in the request.
    /// </summary>
    /// <short>Save the mail white label settings</short>
    /// <path>api/2.0/settings/rebranding/mail</path>
    [ApiExplorerSettings(IgnoreApi = true)]
    [SwaggerResponse(200, "Boolean value: true if the operation is successful", typeof(bool))]
    [SwaggerResponse(400, "Settings is empty")]
    [SwaggerResponse(403, "No permissions to perform this action")]
    [Tags("Settings / Rebranding")]
    [HttpPost("rebranding/mail")]
    public async Task<bool> SaveMailWhiteLabelSettingsAsync(MailWhiteLabelSettingsWrapper wrapper)
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        await DemandRebrandingPermissionAsync();

        ArgumentNullException.ThrowIfNull(wrapper?.Settings, "settings");

        await settingsManager.SaveForDefaultTenantAsync(wrapper.Settings);

        return true;
    }

    /// <summary>
    /// Returns the mail white label settings.
    /// </summary>
    /// <short>Get the mail white label settings</short>
    /// <path>api/2.0/settings/rebranding/mail</path>
    [ApiExplorerSettings(IgnoreApi = true)]
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Mail white label settings", typeof(MailWhiteLabelSettingsDto))]
    [HttpGet("rebranding/mail")]
    public async Task<MailWhiteLabelSettingsDto> GetMailWhiteLabelSettingsAsync()
    {
        var settings = await settingsManager.LoadForDefaultTenantAsync<MailWhiteLabelSettings>();

        return mapper.Map<MailWhiteLabelSettings, MailWhiteLabelSettingsDto>(settings);
    }

    /// <summary>
    /// Deletes the mail white label settings.
    /// </summary>
    /// <short>Delete the mail white label settings</short>
    /// <path>api/2.0/settings/rebranding/mail</path>
    [ApiExplorerSettings(IgnoreApi = true)]
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Default mail white label settings", typeof(MailWhiteLabelSettings))]
    [SwaggerResponse(403, "No permissions to perform this action")]
    [HttpDelete("rebranding/mail")]
    public async Task<MailWhiteLabelSettings> DeleteMailWhiteLabelSettingsAsync()
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        await DemandRebrandingPermissionAsync(false);

        var defaultSettings = settingsManager.GetDefault<MailWhiteLabelSettings>();

        await settingsManager.SaveForDefaultTenantAsync(defaultSettings);

        return defaultSettings;
    }

    #endregion

    /// <summary>
    /// Checks if the white label is enabled or not.
    /// </summary>
    /// <short>Check the white label availability</short>
    /// <path>api/2.0/settings/enablewhitelabel</path>
    [Tags("Settings / Rebranding")]
    [SwaggerResponse(200, "Boolean value: true if the white label is enabled", typeof(bool))]
    [HttpGet("enablewhitelabel")]
    public async Task<bool> GetEnableWhitelabelAsync()
    {
        await permissionContext.DemandPermissionsAsync(SecurityConstants.EditPortalSettings);

        return await tenantLogoManager.GetEnableWhitelabelAsync();
    }

    private async Task DemandRebrandingPermissionAsync(bool demandWhiteLabelPermission = true)
    {
        await tenantExtra.DemandAccessSpacePermissionAsync();

        if (coreBaseSettings.CustomMode)
        {
            throw new SecurityException(Resource.ErrorAccessDenied);
        }

        if (demandWhiteLabelPermission)
        {
            await tenantLogoManager.DemandWhiteLabelPermissionAsync();
        }
    }
}
