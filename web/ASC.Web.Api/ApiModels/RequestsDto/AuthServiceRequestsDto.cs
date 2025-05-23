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

namespace ASC.Web.Api.ApiModel.RequestsDto;

/// <summary>
/// The request parameters for handling the authorization service.
/// </summary>
public class AuthServiceRequestsDto
{
    /// <summary>
    /// The name of the authentication service.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The user-friendly display title of the authentication service.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The brief description of the authentication service.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The detailed instructions for configuring or using the authentication service.
    /// </summary>
    public string Instruction { get; set; }

    /// <summary>
    /// Specifies whether the authentication service can be configured by the user.
    /// </summary>
    public bool CanSet { get; set; }

    /// <summary>
    /// The collection of authorization keys associated with the authentication service.
    /// </summary>
    public List<AuthKey> Props { get; set; }

    public static async Task<AuthServiceRequestsDto> From(Consumer consumer, string logoText)
    {
        var authService = await AuthService.From(consumer, logoText);
        var result = new AuthServiceRequestsDto 
        { 
            Name = authService.Name, 
            Title = authService.Title, 
            Description = authService.Description, 
            Instruction = authService.Instruction,
            CanSet = authService.CanSet
        };

        if (consumer.CanSet)
        {
            result.Props = authService.Props;
            result.CanSet = authService.CanSet;
        }

        return result;
    }
}