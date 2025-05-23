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

namespace ASC.Web.Files.Core.Entries;

/// <summary>
/// The encryption key pair parameters.
/// </summary>
public class EncryptionKeyPairDto
{
    /// <summary>
    /// The private key.
    /// </summary>
    public string PrivateKeyEnc { get; set; }

    /// <summary>
    /// The public key.
    /// </summary>
    public string PublicKey { get; set; }

    /// <summary>
    /// The user ID of the encryption keys.
    /// </summary>
    public Guid UserId { get; set; }
}

[Scope]
public class EncryptionKeyPairDtoHelper(UserManager userManager,
    AuthContext authContext,
    EncryptionLoginProvider encryptionLoginProvider,
    FileSecurity fileSecurity,
    IDaoFactory daoFactory)
{
    public async Task SetKeyPairAsync(string publicKey, string privateKeyEnc)
    {
        ArgumentException.ThrowIfNullOrEmpty(publicKey);
        ArgumentException.ThrowIfNullOrEmpty(privateKeyEnc);

        var user = await userManager.GetUsersAsync(authContext.CurrentAccount.ID);
        if (!authContext.IsAuthenticated || await userManager.IsGuestAsync(user))
        {
            throw new SecurityException();
        }

        var keyPair = new EncryptionKeyPairDto
        {
            PrivateKeyEnc = privateKeyEnc,
            PublicKey = publicKey,
            UserId = user.Id
        };

        var keyPairString = JsonSerializer.Serialize(keyPair);
        await encryptionLoginProvider.SetKeysAsync(user.Id, keyPairString);
    }

    public async Task<EncryptionKeyPairDto> GetKeyPairAsync()
    {
        var currentAddressString = await encryptionLoginProvider.GetKeysAsync();
        if (string.IsNullOrEmpty(currentAddressString))
        {
            return null;
        }

        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        };
        var keyPair = JsonSerializer.Deserialize<EncryptionKeyPairDto>(currentAddressString, options);
        if (keyPair.UserId != authContext.CurrentAccount.ID)
        {
            return null;
        }

        return keyPair;
    }

    public async Task<IEnumerable<EncryptionKeyPairDto>> GetKeyPairAsync<T>(T fileId, FileStorageService FileStorageService)
    {
        var fileDao = daoFactory.GetFileDao<T>();
        var folderDao = daoFactory.GetFolderDao<T>();

        await fileDao.InvalidateCacheAsync(fileId);

        var file = await fileDao.GetFileAsync(fileId);
        if (file == null)
        {
            throw new FileNotFoundException(FilesCommonResource.ErrorMessage_FileNotFound);
        }

        if (!await fileSecurity.CanEditAsync(file))
        {
            throw new SecurityException(FilesCommonResource.ErrorMessage_SecurityException_EditFile);
        }

        var locatedInPrivateRoom = file.RootFolderType == FolderType.VirtualRooms
            && await DocSpaceHelper.LocatedInPrivateRoomAsync(file, folderDao);

        if (file.RootFolderType != FolderType.Privacy && !locatedInPrivateRoom)
        {
            throw new NotSupportedException();
        }

        var tmpFiles = await FileStorageService.GetSharedInfoAsync(new List<T> { fileId }, new List<T>());
        var fileShares = tmpFiles.ToList();
        fileShares = fileShares.Where(share => !share.SubjectGroup && share.Access == FileShare.ReadWrite).ToList();

        var tasks = fileShares.Select(async share =>
        {
            var fileKeyPairString = await encryptionLoginProvider.GetKeysAsync(share.Id);
            if (string.IsNullOrEmpty(fileKeyPairString))
            {
                return null;
            }

            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };
            var fileKeyPair = JsonSerializer.Deserialize<EncryptionKeyPairDto>(fileKeyPairString, options);
            if (fileKeyPair.UserId != share.Id)
            {
                return null;
            }

            fileKeyPair.PrivateKeyEnc = null;

            return fileKeyPair;
        });

        var fileKeysPair = (await Task.WhenAll(tasks))
            .Where(keyPair => keyPair != null);

        return fileKeysPair;
    }
}
