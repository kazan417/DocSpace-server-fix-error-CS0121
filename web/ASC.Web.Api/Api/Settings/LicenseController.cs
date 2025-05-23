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

[DefaultRoute("license")]
public class LicenseController(ILoggerProvider option,
        MessageService messageService,
        ApiContext apiContext,
        UserManager userManager,
        TenantManager tenantManager,
        TenantLogoManager tenantLogoManager,
        TenantExtra tenantExtra,
        AuthContext authContext,
        LicenseReader licenseReader,
        SettingsManager settingsManager,
        WebItemManager webItemManager,
        CoreBaseSettings coreBaseSettings,
        IFusionCache fusionCache,
        FirstTimeTenantSettings firstTimeTenantSettings,
        ITariffService tariffService,
        IHttpContextAccessor httpContextAccessor,
        DocumentServiceLicense documentServiceLicense)
    : BaseSettingsController(apiContext, fusionCache, webItemManager, httpContextAccessor)
{
    private readonly ILogger _log = option.CreateLogger("ASC.Api");

    /// <summary>
    /// Refreshes the portal license.
    /// </summary>
    /// <short>Refresh the license</short>
    /// <path>api/2.0/settings/license/refresh</path>
    [Tags("Settings / License")]
    [SwaggerResponse(200, "Boolean value: true if the operation is successful", typeof(bool))]
    [HttpGet("refresh")]
    [AllowNotPayment]
    public async Task<bool> RefreshLicenseAsync()
    {
        if (!tenantExtra.Enterprise)
        {
            return false;
        }

        await licenseReader.RefreshLicenseAsync(documentServiceLicense.ValidateLicense);
        return true;
    }

    /// <summary>
    /// Activates a license for the portal.
    /// </summary>
    /// <short>
    /// Activate a license
    /// </short>
    /// <path>api/2.0/settings/license/accept</path>
    [Tags("Settings / License")]
    [SwaggerResponse(200, "Message about the result of activating license", typeof(string))]
    [AllowNotPayment]
    [HttpPost("accept")]
    public async Task<string> AcceptLicenseAsync()
    {
        if (!tenantExtra.Enterprise)
        {
            return Resource.ErrorNotAllowedOption;
        }

        await TariffSettings.SetLicenseAcceptAsync(settingsManager);
        messageService.Send(MessageAction.LicenseKeyUploaded);

        try
        {
            await licenseReader.RefreshLicenseAsync(documentServiceLicense.ValidateLicense);
        }
        catch (BillingNotFoundException)
        {
            return UserControlsCommonResource.LicenseKeyNotFound;
        }
        catch (BillingNotConfiguredException ex)
        {
            _log.ErrorWithException(ex);
            return UserControlsCommonResource.LicenseKeyNotCorrect;
        }
        catch (BillingLicenseTypeException)
        {
            var logoText = await tenantLogoManager.GetLogoTextAsync();
            return string.Format(UserControlsCommonResource.LicenseTypeNotCorrect, logoText);
        }
        catch (BillingException)
        {
            return UserControlsCommonResource.LicenseException;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }

        return "";
    }

    /// <summary>
    /// Activates a trial license for the portal.
    /// </summary>
    /// <short>
    /// Activate a trial license
    /// </short>
    /// <path>api/2.0/settings/license/trial</path>
    [ApiExplorerSettings(IgnoreApi = true)]
    [Tags("Settings / License")]
    [SwaggerResponse(200, "Boolean value: true if the operation is successful", typeof(bool))]
    [SwaggerResponse(403, "No permissions to perform this action")]
    [HttpPost("trial")]
    public async Task<bool> ActivateTrialAsync()
    {
        if (!coreBaseSettings.Standalone)
        {
            throw new NotSupportedException();
        }

        if (!await userManager.IsDocSpaceAdminAsync(authContext.CurrentAccount.ID))
        {
            throw new SecurityException();
        }

        var curQuota = await tenantManager.GetCurrentTenantQuotaAsync();
        if (curQuota.TenantId != Tenant.DefaultTenant)
        {
            return false;
        }

        if (curQuota.Trial)
        {
            return false;
        }

        var curTariff = await tenantExtra.GetCurrentTariffAsync();
        if (curTariff.DueDate.Date != DateTime.MaxValue.Date)
        {
            return false;
        }

        var quota = new TenantQuota(-1000)
        {
            Name = "apirequest",
            CountUser = curQuota.CountUser,
            MaxFileSize = curQuota.MaxFileSize,
            MaxTotalSize = curQuota.MaxTotalSize,
            Features = curQuota.Features,
            Trial = true
        };

        await tenantManager.SaveTenantQuotaAsync(quota);

        const int DEFAULT_TRIAL_PERIOD = 30;

        var tariff = new Tariff
        {
            Quotas = [new Quota(quota.TenantId, 1)],
            DueDate = DateTime.Today.AddDays(DEFAULT_TRIAL_PERIOD)
        };

        await tariffService.SetTariffAsync(Tenant.DefaultTenant, tariff, [quota]);

        messageService.Send(MessageAction.LicenseKeyUploaded);

        return true;
    }

    /// <summary>
    /// Requests a portal license if necessary.
    /// </summary>
    /// <short>
    /// Request a license
    /// </short>
    /// <path>api/2.0/settings/license/required</path>
    /// <requiresAuthorization>false</requiresAuthorization>\
    [Tags("Settings / License")]
    [SwaggerResponse(200, "Boolean value: true if the license is required", typeof(bool))]
    [AllowAnonymous]
    [AllowNotPayment]
    [HttpGet("required")]
    public async Task<bool> RequestLicense()
    {
        return await firstTimeTenantSettings.GetRequestLicense();
    }


    /// <summary>
    /// Uploads a portal license specified in the request.
    /// </summary>
    /// <short>
    /// Upload a license
    /// </short>
    /// <path>api/2.0/settings/license</path>
    [Tags("Settings / License")]
    [SwaggerResponse(200, "License", typeof(string))]
    [SwaggerResponse(400, "The uploaded file could not be found")]
    [SwaggerResponse(403, "Portal Access")]
    [SwaggerResponse(405, "Your pricing plan does not support this option")]
    [AllowNotPayment]
    [HttpPost("")]
    [Authorize(AuthenticationSchemes = "confirm", Roles = "Wizard, Administrators")]
    public async Task<string> UploadLicenseAsync([FromForm] UploadLicenseRequestsDto inDto)
    {
        try
        {
            await ApiContext.AuthByClaimAsync();
            if (!authContext.IsAuthenticated && (await settingsManager.LoadAsync<WizardSettings>()).Completed)
            {
                throw new SecurityException(Resource.PortalSecurity);
            }

            if (!tenantExtra.Enterprise)
            {
                throw new NotSupportedException(Resource.ErrorNotAllowedOption);
            }

            if (!inDto.Files.Any())
            {
                throw new Exception(Resource.ErrorEmptyUploadFileSelected);
            }

            var licenseFile = inDto.Files.First();
            var dueDate = await licenseReader.SaveLicenseTemp(licenseFile.OpenReadStream());

            return dueDate >= DateTime.UtcNow.Date
                                    ? Resource.LicenseUploaded
                                    : string.Format(
                                        (await tenantManager.GetCurrentTenantQuotaAsync()).Update
                                            ? Resource.LicenseUploadedOverdueSupport
                                            : Resource.LicenseUploadedOverdue,
                                                    "",
                                                    "",
                                                    dueDate.Date.ToLongDateString());
        }
        catch (SecurityException ex)
        {
            _log.ErrorLicenseUpload(ex);
            throw;
        }
        catch (NotSupportedException ex)
        {
            _log.ErrorLicenseUpload(ex);
            throw;
        }
        catch (LicenseExpiredException ex)
        {
            _log.ErrorLicenseUpload(ex);
            throw new Exception(Resource.LicenseErrorExpired);
        }
        catch (LicenseQuotaException ex)
        {
            _log.ErrorLicenseUpload(ex);
            throw new Exception(Resource.LicenseErrorQuota);
        }
        catch (LicensePortalException ex)
        {
            _log.ErrorLicenseUpload(ex);
            throw new Exception(Resource.LicenseErrorPortal);
        }
        catch (BillingLicenseTypeException ex)
        {
            _log.ErrorLicenseUpload(ex);
            var logoText = await tenantLogoManager.GetLogoTextAsync();
            throw new Exception(string.Format(UserControlsCommonResource.LicenseTypeNotCorrect, logoText));
        }
        catch (Exception ex)
        {
            _log.ErrorLicenseUpload(ex);
            throw new Exception(Resource.LicenseError);
        }
    }
}