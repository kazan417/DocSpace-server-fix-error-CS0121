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

namespace ASC.Web.Api.ApiModel.ResponseDto;

/// <summary>
/// The login event parameters.
/// </summary>
public class LoginEventDto(LoginEvent loginEvent, ApiDateTimeHelper apiDateTimeHelper)
{
    /// <summary>
    /// The login event ID.
    /// </summary>
    public int Id { get; set; } = loginEvent.Id;

    /// <summary>
    /// The login event date.
    /// </summary>
    public ApiDateTime Date { get; set; } = apiDateTimeHelper.Get(loginEvent.Date);

    /// <summary>
    /// The user name of the login event.
    /// </summary>
    public string User { get; set; } = loginEvent.UserName;

    /// <summary>
    /// The user ID of the login event.
    /// </summary>
    public Guid UserId { get; set; } = loginEvent.UserId;

    /// <summary>
    /// The user login of the login event.
    /// </summary>
    public string Login { get; set; } = loginEvent.Login;

    /// <summary>
    /// The login event action.
    /// </summary>
    public string Action { get; set; } = loginEvent.ActionText;

    /// <summary>
    /// The login-related action to filter events by.
    /// </summary>
    public MessageAction ActionId { get; set; } = (MessageAction)loginEvent.Action;

    /// <summary>
    /// The login event IP.
    /// </summary>
    public string IP { get; set; } = loginEvent.IP;

    /// <summary>
    /// The login event country.
    /// </summary>
    public string Country { get; set; } = loginEvent.Country;

    /// <summary>
    /// The login event city.
    /// </summary>
    public string City { get; set; } = loginEvent.City;

    /// <summary>
    /// The login event browser.
    /// </summary>
    public string Browser { get; set; } = loginEvent.Browser;

    /// <summary>
    /// The login event platform.
    /// </summary>
    public string Platform { get; set; } = loginEvent.Platform;

    /// <summary>
    /// The login event page.
    /// </summary>
    public string Page { get; set; } = loginEvent.Page;
}