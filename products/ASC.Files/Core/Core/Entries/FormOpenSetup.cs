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

namespace ASC.Files.Core.Core.Entries;

/// <summary>
/// The settings for opening the form document.
/// </summary>
public record FormOpenSetup<T>
{
    /// <summary>
    /// Specifies if the form can be edited or not.
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Specifies if the form can be filled out or not.
    /// </summary>
    public bool CanFill { get; set; }

    /// <summary>
    /// Specifies if the "Start filling" button is displayed in the form or not.
    /// </summary>
    public bool CanStartFilling { get; set; } = true;

    /// <summary>
    /// Specifies if the completed form can be submitted only or not.
    /// </summary>
    public bool IsSubmitOnly { get; set; }
    
    /// <summary>
    /// The form filling session ID.
    /// </summary>
    public string FillingSessionId { get; set; }
    
    /// <summary>
    /// The editor type.
    /// </summary>
    public EditorType EditorType { get; set; }
    
    /// <summary>
    /// The form draft parameters.
    /// </summary>
    public File<T> Draft { get; set; }
    
    /// <summary>
    /// The role name of the user who fills out the form.
    /// </summary>
    public string RoleName { get; set; }
    
    /// <summary>
    /// The root folder where the current form is located.
    /// </summary>
    public Folder<T> RootFolder { get; set; }

}