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

namespace ASC.Web.Api.ApiModels.ResponseDto;

/// <summary>
/// The webhook log parameters.
/// </summary>
public class WebhooksLogDto : IMapFrom<DbWebhooksLog>
{
    /// <summary>
    /// The webhook log ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The webhook configuration name.
    /// </summary>
    public string ConfigName { get; set; }

    /// <summary>
    /// The webhook trigger type.
    /// </summary>
    public WebhookTrigger Trigger { get; set; }

    /// <summary>
    /// The webhook creation time.
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// The webhook method.
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// The webhook route.
    /// </summary>
    public string Route { get; set; }

    /// <summary>
    /// The webhook request headers.
    /// </summary>
    public string RequestHeaders { get; set; }

    /// <summary>
    /// The webhook request payload.
    /// </summary>
    public string RequestPayload { get; set; }

    /// <summary>
    /// The webhook response headers.
    /// </summary>
    public string ResponseHeaders { get; set; }

    /// <summary>
    /// The webhook response payload.
    /// </summary>
    public string ResponsePayload { get; set; }

    /// <summary>
    /// The webhook status.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// The webhook delivery time.
    /// </summary>
    public DateTime? Delivery { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<DbWebhooksLog, WebhooksLogDto>()
              .ConvertUsing<WebhooksLogConverter>();
    }
}

[Scope]
public class WebhooksLogConverter(TenantUtil tenantUtil) : ITypeConverter<DbWebhooksLog, WebhooksLogDto>
{
    public WebhooksLogDto Convert(DbWebhooksLog source, WebhooksLogDto destination, ResolutionContext context)
    {
        var result = new WebhooksLogDto
        {
             Id = source.Id,
             CreationTime = tenantUtil.DateTimeFromUtc(source.CreationTime),
             Status = source.Status,
             RequestHeaders = source.RequestHeaders,
             RequestPayload = source.RequestPayload,
             ResponseHeaders = source.ResponseHeaders,
             ResponsePayload = source.ResponsePayload,
             Trigger = source.Trigger
        };

        if (source.Delivery.HasValue)
        {
            result.Delivery = tenantUtil.DateTimeFromUtc(source.Delivery.Value);
        }
        
        return result;
    }
}