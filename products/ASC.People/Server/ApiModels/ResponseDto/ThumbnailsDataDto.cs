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

namespace ASC.People.ApiModels.ResponseDto;

///<summary>
/// The thumbnails data parameters.
///</summary>
public class ThumbnailsDataDto
{
    private ThumbnailsDataDto() { }

    public static async Task<ThumbnailsDataDto> Create(UserInfo userInfo, UserPhotoManager userPhotoManager)
    {
        var cacheKey = Math.Abs(userInfo.LastModified.GetHashCode());

        return new ThumbnailsDataDto
        {
            Original = await userPhotoManager.GetPhotoAbsoluteWebPath(userInfo.Id) + $"?hash={cacheKey}",
            Retina = await userPhotoManager.GetRetinaPhotoURL(userInfo.Id) + $"?hash={cacheKey}",
            Max = await userPhotoManager.GetMaxPhotoURL(userInfo.Id) + $"?hash={cacheKey}",
            Big = await userPhotoManager.GetBigPhotoURL(userInfo.Id) + $"?hash={cacheKey}",
            Medium = await userPhotoManager.GetMediumPhotoURL(userInfo.Id) + $"?hash={cacheKey}",
            Small = await userPhotoManager.GetSmallPhotoURL(userInfo.Id) + $"?hash={cacheKey}"
        };
    }

    /// <summary>
    /// The thumbnail original photo.
    /// </summary>
    [SwaggerSchemaCustom(Example = "default_user_photo_size_1280-1280.png")]
    public string Original { get; set; }

    /// <summary>
    /// The thumbnail retina.
    /// </summary>
    [SwaggerSchemaCustom(Example = "default_user_photo_size_360-360.png")]
    public string Retina { get; set; }

    /// <summary>
    /// The thumbnail maximum size photo.
    /// </summary>
    [SwaggerSchemaCustom(Example = "default_user_photo_size_200-200.png")]
    public string Max { get; set; }

    /// <summary>
    /// The thumbnail big size photo.
    /// </summary>
    [SwaggerSchemaCustom(Example = "default_user_photo_size_82-82.png")]
    public string Big { get; set; }

    /// <summary>
    /// The thumbnail medium size photo.
    /// </summary>
    [SwaggerSchemaCustom(Example = "default_user_photo_size_48-48.png")]
    public string Medium { get; set; }

    /// <summary>
    /// The thumbnail small size photo.
    /// </summary>
    [SwaggerSchemaCustom(Example = "default_user_photo_size_32-32.png")]
    public string Small { get; set; }
}
