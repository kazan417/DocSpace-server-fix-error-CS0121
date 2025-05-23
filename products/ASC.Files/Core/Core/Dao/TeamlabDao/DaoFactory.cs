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

namespace ASC.Files.Core.Data;

[Scope(typeof(IDaoFactory))]
public class DaoFactory(IServiceProvider serviceProvider, IProviderDao providerDao) : IDaoFactory
{
    public IProviderDao ProviderDao { get; } = providerDao;

    public IFileDao<T> GetFileDao<T>()
    {
        return serviceProvider.GetService<IFileDao<T>>();
    }

    public IFileDao<T> GetCacheFileDao<T>()
    {
        return serviceProvider.GetService<ICacheFileDao<T>>() ??
               serviceProvider.GetService<IFileDao<T>>();
    }

    public IFolderDao<T> GetFolderDao<T>()
    {
        return serviceProvider.GetService<IFolderDao<T>>();
    }

    public IFolderDao<T> GetCacheFolderDao<T>()
    {
        return serviceProvider.GetService<ICacheFolderDao<T>>() ?? 
               serviceProvider.GetService<IFolderDao<T>>();
    }

    public ITagDao<T> GetTagDao<T>()
    {
        return serviceProvider.GetService<ITagDao<T>>();
    }

    public ISecurityDao<T> GetSecurityDao<T>()
    {
        return serviceProvider.GetService<ISecurityDao<T>>();
    }

    public ILinkDao<T> GetLinkDao<T>()
    {
        return serviceProvider.GetService<ILinkDao<T>>();
    }

    public IMappingId<T> GetMapping<T>()
    {
        return serviceProvider.GetService<IMappingId<T>>();
    }
}