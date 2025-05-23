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

[DefaultRoute("tips")]
[ApiExplorerSettings(IgnoreApi = true)]
public class TipsController(ILoggerProvider option,
        ApiContext apiContext,
        AuthContext authContext,
        StudioNotifyHelper studioNotifyHelper,
        SettingsManager settingsManager,
        WebItemManager webItemManager,
        SetupInfo setupInfo,
        IFusionCache fusionCache,
        IHttpClientFactory clientFactory,
        TenantManager tenantManager,
        IHttpContextAccessor httpContextAccessor)
    : BaseSettingsController(apiContext, fusionCache, webItemManager, httpContextAccessor)
{
    private readonly ILogger _log = option.CreateLogger("ASC.Api");

    /// <summary>
    /// Updates the user interface tip settings with the parameters specified in the request.
    /// </summary>
    /// <short>Update the tip settings</short>
    /// <path>api/2.0/settings/tips</path>
    [Tags("Settings / Tips")]
    [SwaggerResponse(200, "Updated tip settings", typeof(TipsSettings))]
    [HttpPut("")]
    public async Task<TipsSettings> UpdateTipsSettingsAsync(TipsRequestDto inDto)
    {
        var settings = new TipsSettings { Show = inDto.Show };
        await settingsManager.SaveForCurrentUserAsync(settings);

        if (!inDto.Show && !string.IsNullOrEmpty(setupInfo.TipsAddress))
        {
            try
            {
                var tenant = tenantManager.GetCurrentTenant();
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri($"{setupInfo.TipsAddress}/tips/deletereaded")
                };

                var data = new NameValueCollection
                {
                    ["userId"] = authContext.CurrentAccount.ID.ToString(),
                    ["tenantId"] = tenant.Id.ToString(CultureInfo.InvariantCulture)
                };
                var body = JsonSerializer.Serialize(data);//todo check
                request.Content = new StringContent(body);

                var httpClient = clientFactory.CreateClient();
                using var response = await httpClient.SendAsync(request);

            }
            catch (Exception e)
            {
                _log.ErrorWithException(e);
            }
        }

        return settings;
    }

    /// <summary>
    /// Updates the tip subscription.
    /// </summary>
    /// <short>Update the tip subscription</short>
    /// <path>api/2.0/settings/tips/change/subscription</path>
    [Tags("Settings / Tips")]
    [SwaggerResponse(200, "Boolean value: true if the user is subscribed to the tips", typeof(bool))]
    [HttpPut("change/subscription")]
    public async Task<bool> UpdateTipsSubscriptionAsync()
    {
        return await StudioPeriodicNotify.ChangeSubscriptionAsync(authContext.CurrentAccount.ID, studioNotifyHelper);
    }

    /// <summary>
    /// Checks if the current user is subscribed to the tips or not.
    /// </summary>
    /// <short>Check the tip subscription</short>
    /// <path>api/2.0/settings/tips/subscription</path>
    [Tags("Settings / Tips")]
    [SwaggerResponse(200, "Boolean value: true if the user is subscribed to the tips", typeof(bool))]
    [HttpGet("subscription")]
    public async Task<bool> GetTipsSubscriptionAsync()
    {
        return await studioNotifyHelper.IsSubscribedToNotifyAsync(authContext.CurrentAccount.ID, Actions.PeriodicNotify);
    }
}
