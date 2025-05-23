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

namespace ASC.Files.Api;

[ConstraintRoute("int")]
public class MasterFormControllerInternal(FileStorageService fileStorageServiceString,
        FolderDtoHelper folderDtoHelper,
        FileDtoHelper fileDtoHelper)
    : MasterFormController<int>(fileStorageServiceString, folderDtoHelper, fileDtoHelper);

public class MasterFormControllerThirdparty(FileStorageService fileStorageService,
        FolderDtoHelper folderDtoHelper,
        FileDtoHelper fileDtoHelper)
    : MasterFormController<string>(fileStorageService, folderDtoHelper, fileDtoHelper);

public abstract class MasterFormController<T>(FileStorageService fileStorageService,
        FolderDtoHelper folderDtoHelper,
        FileDtoHelper fileDtoHelper)
    : ApiControllerBase(folderDtoHelper, fileDtoHelper)
    {
    /// <summary>
    /// Checks if the current file is a form draft which can be filled out.
    /// </summary>
    /// <short>Check the form draft filling</short>
    /// <path>api/2.0/files/masterform/{fileId}/checkfillformdraft</path>
    /// <requiresAuthorization>false</requiresAuthorization>
    [Tags("Files / Files")]
    [SwaggerResponse(200, "Link to the form", typeof(string))]
    [SwaggerResponse(403, "You don't have enough permission to view the file")]
    [AllowAnonymous]
    [HttpPost("masterform/{fileId}/checkfillformdraft")]
    public async Task<string> CheckFillFormDraftAsync(CheckFillFormDraftRequestDto<T> inDto)
    {
        return await fileStorageService.CheckFillFormDraftAsync(inDto.FileId, inDto.File.Version,!inDto.File.RequestEmbedded, inDto.File.RequestView);
    }
}