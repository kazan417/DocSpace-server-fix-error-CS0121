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

[DefaultRoute("fileops")]
public class OperationController(
    FileOperationDtoHelper fileOperationDtoHelper,
    FolderDtoHelper folderDtoHelper,
    FileDtoHelper fileDtoHelper,
    FileStorageService fileStorageService,
    FileDownloadOperationsManager fileDownloadOperationsManager, 
    FileMoveCopyOperationsManager fileMoveCopyOperationsManager, 
    FileDeleteOperationsManager fileDeleteOperationsManager, 
    FileMarkAsReadOperationsManager fileMarkAsReadOperationsManager, 
    FileDuplicateOperationsManager fileDuplicateOperationsManager, 
    CommonLinkUtility commonLinkUtility)
    : ApiControllerBase(folderDtoHelper, fileDtoHelper)
{
    /// <summary>
    /// Starts the download process of files and folders with the IDs specified in the request.
    /// </summary>
    /// <short>Bulk download</short>
    /// <path>api/2.0/files/fileops/bulkdownload</path>
    /// <collection>list</collection>
    /// <requiresAuthorization>false</requiresAuthorization>
    [Tags("Files / Operations")]
    [SwaggerResponse(200, "List of file operations", typeof(IAsyncEnumerable<FileOperationDto>))]
    [SwaggerResponse(403, "You don't have enough permission to download")]
    [AllowAnonymous]
    [HttpPut("bulkdownload")]
    public async IAsyncEnumerable<FileOperationDto> BulkDownload(DownloadRequestDto inDto)
    {
        var files = inDto.FileConvertIds.Select(fileId => new FilesDownloadOperationItem<JsonElement>(fileId.Key, fileId.Value, fileId.Password)).ToList();
        files.AddRange(inDto.FileIds.Select(fileId => new FilesDownloadOperationItem<JsonElement>(fileId, string.Empty, string.Empty)));

        await fileDownloadOperationsManager.Publish(inDto.FolderIds, files, commonLinkUtility.ServerRootPath);

        foreach (var e in await fileDownloadOperationsManager.GetOperationResults())
        {
            yield return await fileOperationDtoHelper.GetAsync(e);
        }
    }

    /// <summary>
    /// Copies all the selected files and folders to the folder with the ID specified in the request.
    /// </summary>
    /// <short>Copy to the folder</short>
    /// <path>api/2.0/files/fileops/copy</path>
    /// <collection>list</collection>
    [Tags("Files / Operations")]
    [SwaggerResponse(200, "List of file operations", typeof(IAsyncEnumerable<FileOperationDto>))]
    [SwaggerResponse(403, "You don't have enough permission to copy")]
    [HttpPut("copy")]
    public async IAsyncEnumerable<FileOperationDto> CopyBatchItems(BatchRequestDto inDto)
    {
        await fileMoveCopyOperationsManager.Publish(inDto.FolderIds, inDto.FileIds, inDto.DestFolderId, true, inDto.ConflictResolveType, !inDto.DeleteAfter, inDto.ToFillOut, inDto.Content);
        
        foreach (var e in (await fileMoveCopyOperationsManager.GetOperationResults()).Where(r=> r.OperationType == FileOperationType.Copy))
        {
            yield return await fileOperationDtoHelper.GetAsync(e);
        }
    }

    /// <summary>
    /// Deletes the files and folders with the IDs specified in the request.
    /// </summary>
    /// <short>Delete files and folders</short>
    /// <path>api/2.0/files/fileops/delete</path>
    /// <collection>list</collection>
    [Tags("Files / Operations")]
    [SwaggerResponse(200, "List of file operations", typeof(IAsyncEnumerable<FileOperationDto>))]
    [SwaggerResponse(403, "You don't have enough permission to delete")]
    [HttpPut("delete")]
    public async IAsyncEnumerable<FileOperationDto> DeleteBatchItems(DeleteBatchRequestDto inDto)
    {
        await fileDeleteOperationsManager.Publish(inDto.FolderIds, inDto.FileIds, false, !inDto.DeleteAfter, inDto.Immediately);
        
        foreach (var e in await fileDeleteOperationsManager.GetOperationResults())
        {
            yield return await fileOperationDtoHelper.GetAsync(e);
        }
    }

    /// <summary>
    /// Deletes the file versions with the IDs specified in the request.
    /// </summary>
    /// <short>Delete file versions</short>
    /// <path>api/2.0/files/fileops/deleteversion</path>
    /// <collection>list</collection>
    [Tags("Files / Operations")]
    [SwaggerResponse(200, "List of file operations", typeof(FileOperationDto))]
    [HttpPut("deleteversion")]
    public async IAsyncEnumerable<FileOperationDto> DeleteFileVersions(DeleteVersionBatchRequestDto inDto)
    {
        await fileDeleteOperationsManager.Publish([], [inDto.FileId], false, !inDto.DeleteAfter, true, versions: inDto.Versions);
        
        foreach (var e in await fileDeleteOperationsManager.GetOperationResults())
        {
            yield return await fileOperationDtoHelper.GetAsync(e);
        }
    }

    /// <summary>
    /// Deletes all the files and folders from the "Trash" folder.
    /// </summary>
    /// <short>Empty the "Trash" folder</short>
    /// <path>api/2.0/files/fileops/emptytrash</path>
    /// <collection>list</collection>
    [Tags("Files / Operations")]
    [SwaggerResponse(200, "List of file operations", typeof(IAsyncEnumerable<FileOperationDto>))]
    [HttpPut("emptytrash")]
    public async IAsyncEnumerable<FileOperationDto> EmptyTrashAsync()
    {
        var (foldersId, filesId) = await fileStorageService.GetTrashContentAsync();
        
        await fileDeleteOperationsManager.Publish(foldersId, filesId, false, true, false, true);

        foreach (var e in await fileDeleteOperationsManager.GetOperationResults())
        {
            yield return await fileOperationDtoHelper.GetAsync(e);
        }
    }

    /// <summary>
    ///  Returns a list of all the active file operations.
    /// </summary>
    /// <short>Get active file operations</short>
    /// <path>api/2.0/files/fileops</path>
    /// <collection>list</collection>
    /// <requiresAuthorization>false</requiresAuthorization>
    [Tags("Files / Operations")]
    [SwaggerResponse(200, "List of file operations", typeof(IAsyncEnumerable<FileOperationDto>))]
    [AllowAnonymous]
    [HttpGet("")]
    public async IAsyncEnumerable<FileOperationDto> GetOperationStatuses()
    {
        List<IFileOperationsManager> managers = [fileDuplicateOperationsManager, fileMoveCopyOperationsManager, fileDeleteOperationsManager, fileDownloadOperationsManager, fileMarkAsReadOperationsManager];

        foreach (var manager in managers)
        {
            foreach (var e in await manager.GetOperationResults())
            {
                yield return await fileOperationDtoHelper.GetAsync(e);
            }
        }
    }

    /// <summary>
    /// Retrieves the statuses of operations filtered by the specified operation type.
    /// </summary>
    /// <short>Get file operation statuses</short>
    /// <path>api/2.0/files/fileops/{operationType}</path>
    /// <collection>list</collection>
    /// <requiresAuthorization>false</requiresAuthorization>
    [Tags("Files / Operations")]
    [SwaggerResponse(200, "List of file operations", typeof(IAsyncEnumerable<FileOperationDto>))]
    [AllowAnonymous]
    [HttpGet("{operationType}")]
    public async IAsyncEnumerable<FileOperationDto> GetOperationStatusesByType(FileOperationResultRequestDto inDto)
    {
        IFileOperationsManager manager = inDto.OperationType switch
        {
            FileOperationType.Move or FileOperationType.Copy => fileMoveCopyOperationsManager,
            FileOperationType.Delete => fileDeleteOperationsManager,
            FileOperationType.Download => fileDownloadOperationsManager,
            FileOperationType.MarkAsRead => fileMarkAsReadOperationsManager,
            FileOperationType.Duplicate => fileDuplicateOperationsManager,
            _ => throw new ArgumentOutOfRangeException()
        };

        foreach (var e in (await manager.GetOperationResults()).Where(r => r.OperationType == inDto.OperationType).ToList())
        {
            yield return await fileOperationDtoHelper.GetAsync(e);
        }
    }

    /// <summary>
    /// Marks the files and folders with the IDs specified in the request as read.
    /// </summary>
    /// <short>Mark as read</short>
    /// <path>api/2.0/files/fileops/markasread</path>
    /// <collection>list</collection>
    [Tags("Files / Operations")]
    [SwaggerResponse(200, "List of file operations", typeof(IAsyncEnumerable<FileOperationDto>))]
    [HttpPut("markasread")]
    public async IAsyncEnumerable<FileOperationDto> MarkAsRead(BaseBatchRequestDto inDto)
    {
        await fileMarkAsReadOperationsManager.Publish(inDto.FolderIds, inDto.FileIds);
        
        foreach (var e in await fileMarkAsReadOperationsManager.GetOperationResults())
        {
            yield return await fileOperationDtoHelper.GetAsync(e);
        }
    }

    /// <summary>
    /// Moves or copies all the selected files and folders to the folder with the ID specified in the request.
    /// </summary>
    /// <short>Move or copy to a folder</short>
    /// <path>api/2.0/files/fileops/move</path>
    /// <collection>list</collection>
    [Tags("Files / Operations")]
    [SwaggerResponse(200, "List of file operations", typeof(IAsyncEnumerable<FileOperationDto>))]
    [SwaggerResponse(403, "You don't have enough permission to move")]
    [HttpPut("move")]
    public async IAsyncEnumerable<FileOperationDto> MoveBatchItems(BatchRequestDto inDto)
    {
        await fileMoveCopyOperationsManager.Publish(inDto.FolderIds, inDto.FileIds, inDto.DestFolderId, false, inDto.ConflictResolveType, !inDto.DeleteAfter, inDto.ToFillOut, inDto.Content);
        
        foreach (var e in (await fileMoveCopyOperationsManager.GetOperationResults()).Where(r => r.OperationType == FileOperationType.Move))
        {
            yield return await fileOperationDtoHelper.GetAsync(e);
        }
    }

    /// <summary>
    /// Duplicates all the selected files and folders.
    /// </summary>
    /// <short>Duplicate files and folders</short>
    /// <path>api/2.0/files/fileops/duplicate</path>
    /// <collection>list</collection>
    [Tags("Files / Operations")]
    [SwaggerResponse(200, "List of file operations", typeof(IAsyncEnumerable<FileOperationDto>))]
    [SwaggerResponse(403, "You don't have enough permission to duplicate")]
    [HttpPut("duplicate")]
    public async IAsyncEnumerable<FileOperationDto> DuplicateBatchItems(DuplicateRequestDto inDto)
    {
        await fileDuplicateOperationsManager.Publish(inDto.FolderIds, inDto.FileIds);
        
        foreach (var e in await fileDuplicateOperationsManager.GetOperationResults())
        {
            yield return await fileOperationDtoHelper.GetAsync(e);
        }
    }

    /// <summary>
    /// Checks if files can be moved or copied to the specified folder.
    /// </summary>
    /// <short>Check for moving or copying to a folder</short>
    /// <path>api/2.0/files/fileops/checkdestfolder</path>
    [Tags("Files / Operations")]
    [SwaggerResponse(200, "Result", typeof(CheckDestFolderDto))]
    [SwaggerResponse(403, "You don't have enough permission to create")]
    [HttpGet("checkdestfolder")]
    public async Task<CheckDestFolderDto> MoveOrCopyDestFolderCheckAsync([ModelBinder(BinderType = typeof(BatchModelBinder))] BatchRequestDto inDto)
    {
        List<object> checkedFiles;

        if (inDto.DestFolderId.ValueKind == JsonValueKind.Number)
        {
            checkedFiles = await fileStorageService.MoveOrCopyDestFolderCheckAsync(inDto.FileIds.ToList(), inDto.DestFolderId.GetInt32());
        }
        else
        {
            checkedFiles = await fileStorageService.MoveOrCopyDestFolderCheckAsync(inDto.FileIds.ToList(), inDto.DestFolderId.GetString());
        }

        var entries = await fileStorageService.GetItemsAsync(checkedFiles.Select(Convert.ToInt32), checkedFiles.Select(Convert.ToInt32), FilterType.FilesOnly, false);
        entries.AddRange(await fileStorageService.GetItemsAsync(checkedFiles.OfType<string>(), [], FilterType.FilesOnly, false));

        var filesTask = GetFilesDto(entries).ToListAsync();

        var result = inDto.FileIds.Count() - entries.Count != 0 ?
                     (entries.Count != 0 ? CheckDestFolderResult.PartAllowed : CheckDestFolderResult.NoneAllowed) : CheckDestFolderResult.AllAllowed;

        return new CheckDestFolderDto
        {
            Result = result,
            Files = await filesTask
        };

        async IAsyncEnumerable<FileEntryDto> GetFilesDto(IEnumerable<FileEntry> fileEntries)
        {
            foreach (var entry in fileEntries)
            {
                yield return await GetFileEntryWrapperAsync(entry);
            }
        }
    }

    /// <summary>
    /// Checks if files or folders can be moved or copied to the specified folder, moves or copies them, and returns their information.
    /// </summary>
    /// <short>Check and move or copy to a folder</short>
    /// <path>api/2.0/files/fileops/move</path>
    /// <collection>list</collection>
    [Tags("Files / Operations")]
    [SwaggerResponse(200, "List of file entry information", typeof(IAsyncEnumerable<FileEntryDto>))]
    [SwaggerResponse(403, "You don't have enough permission to create")]
    [HttpGet("move")]
    public async IAsyncEnumerable<FileEntryDto> MoveOrCopyBatchCheckAsync([ModelBinder(BinderType = typeof(BatchModelBinder))] BatchRequestDto inDto)
    {
        List<object> checkedFiles;
        List<object> checkedFolders;

        if (inDto.DestFolderId.ValueKind == JsonValueKind.Number)
        {
            (checkedFiles, checkedFolders) = await fileStorageService.MoveOrCopyFilesCheckAsync(inDto.FileIds.ToList(), inDto.FolderIds.ToList(), inDto.DestFolderId.GetInt32());
        }
        else
        {
            (checkedFiles, checkedFolders) = await fileStorageService.MoveOrCopyFilesCheckAsync(inDto.FileIds.ToList(), inDto.FolderIds.ToList(), inDto.DestFolderId.GetString());
        }

        var entries = await fileStorageService.GetItemsAsync(checkedFiles.OfType<int>().Select(Convert.ToInt32), checkedFolders.OfType<int>().Select(Convert.ToInt32), FilterType.None, false);
        entries.AddRange(await fileStorageService.GetItemsAsync(checkedFiles.OfType<string>(), checkedFolders.OfType<string>(), FilterType.None, false));
        
        foreach (var e in entries)
        {
            yield return await GetFileEntryWrapperAsync(e);
        }
    }

    /// <summary>
    /// Finishes an operation with the ID specified in the request or all the active operations.
    /// </summary>
    /// <short>Finish active operations</short>
    /// <path>api/2.0/files/fileops/terminate/{id}</path>
    /// <collection>list</collection>
    /// <requiresAuthorization>false</requiresAuthorization>
    [Tags("Files / Operations")]
    [SwaggerResponse(200, "List of file operations", typeof(IAsyncEnumerable<FileOperationDto>))]
    [AllowAnonymous]
    [HttpPut("terminate/{id?}")]
    public async IAsyncEnumerable<FileOperationDto> TerminateTasks(OperationIdRequestDto inDto)
    {
        List<IFileOperationsManager> managers = [fileDuplicateOperationsManager, fileMoveCopyOperationsManager, fileDeleteOperationsManager, fileDownloadOperationsManager, fileMarkAsReadOperationsManager];

        foreach (var manager in managers)
        {
            foreach (var e in await manager.CancelOperations(inDto.Id))
            {
                yield return await fileOperationDtoHelper.GetAsync(e);
            }
        }
    }
}