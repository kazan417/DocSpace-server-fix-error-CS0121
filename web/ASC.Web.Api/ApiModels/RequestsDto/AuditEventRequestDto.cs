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

namespace ASC.Web.Api.ApiModels.RequestsDto;

/// <summary>
/// The request parameters for filtering and retrieving audit event records.
/// </summary>
public class AuditEventRequestDto
{
    /// <summary>
    /// The ID of the user who triggered the audit event.
    /// </summary>
    [FromQuery(Name = "userId")]
    public Guid UserId { get; set; }

    /// <summary>
    /// The type of product related to the audit event.
    /// </summary>
    [FromQuery(Name = "productType")]
    public ProductType ProductType { get; set; }

    /// <summary>
    /// The module within the product where the audit event occurred.
    /// </summary>
    [FromQuery(Name = "moduleType")]
    public ModuleType ModuleType { get; set; }

    /// <summary>
    /// The type of action performed in the audit event (e.g., Create, Update, Delete).
    /// </summary>
    [FromQuery(Name = "actionType")]
    public ActionType ActionType { get; set; }

    /// <summary>
    /// The specific action that occurred within the audit event.
    /// </summary>
    [FromQuery(Name = "action")]
    public MessageAction Action { get; set; }

    /// <summary>
    /// The type of audit entry (e.g., Folder, User, File).
    /// </summary>
    [FromQuery(Name = "entryType")]
    public EntryType EntryType { get; set; }

    /// <summary>
    /// The target object affected by the audit event (e.g., document ID, user account).
    /// </summary>
    [FromQuery(Name = "target")]
    public string Target {  get; set; }

    /// <summary>
    /// The starting date and time for filtering audit events.
    /// </summary>
    [FromQuery(Name = "from")]
    public ApiDateTime From { get; set; }

    /// <summary>
    /// The ending date and time for filtering audit events.
    /// </summary>
    [FromQuery(Name = "to")]
    public ApiDateTime To { get; set; }
}