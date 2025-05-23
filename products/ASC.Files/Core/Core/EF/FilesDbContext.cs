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

using User = ASC.Core.Common.EF.User;

namespace ASC.Files.Core.EF;

public partial class FilesDbContext(DbContextOptions<FilesDbContext> dbContextOptions) : BaseDbContext(dbContextOptions)
{
    public DbSet<DbFile> Files { get; set; }
    public DbSet<DbFolder> Folders { get; set; }
    public DbSet<DbFolderTree> Tree { get; set; }
    public DbSet<DbFilesBunchObjects> BunchObjects { get; set; }
    public DbSet<DbFilesSecurity> Security { get; set; }
    public DbSet<DbFilesThirdpartyIdMapping> ThirdpartyIdMapping { get; set; }
    public DbSet<DbFilesThirdpartyAccount> ThirdpartyAccount { get; set; }
    public DbSet<DbFilesTagLink> TagLink { get; set; }
    public DbSet<DbFilesTag> Tag { get; set; }
    public DbSet<DbFilesThirdpartyApp> ThirdpartyApp { get; set; }
    public DbSet<DbFilesLink> FilesLink { get; set; }
    public DbSet<DbFilesProperties> FilesProperties { get; set; }
    public DbSet<DbTenant> Tenants { get; set; }
    public DbSet<FilesConverts> FilesConverts { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<DbFileOrder> FileOrder { get; set; }
    public DbSet<DbRoomSettings> RoomSettings { get; set; }
    public DbSet<DbGroup> Groups { get; set; }
    public DbSet<UserGroup> UserGroup { get; set; }
    public DbSet<DbFilesAuditReference> FilesAuditReference { get; set; }
    public DbSet<DbUserRelation> UserRelations { get; set; }
    public DbSet<DbFilesFormRoleMapping> FilesFormRoleMapping { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ModelBuilderWrapper
            .From(modelBuilder, Database)
            .AddDbFiles()
            .AddDbFolder()
            .AddDbFolderTree()
            .AddDbFilesThirdpartyAccount()
            .AddDbFilesBunchObjects()
            .AddDbFilesSecurity()
            .AddDbFilesThirdpartyIdMapping()
            .AddDbFilesFormRoleMapping()
            .AddDbFilesTagLink()
            .AddDbFilesTag()
            .AddDbDbFilesThirdpartyApp()
            .AddDbFilesLink()
            .AddDbFilesProperties()
            .AddDbTenant()
            .AddFilesConverts()
            .AddDbFileOrder()
            .AddUser()
            .AddDbRoomSettings()
            .AddDbGroup()
            .AddUserGroup()
            .AddFilesAuditReference()
            .AddUserRelation()
            .AddDbFunctions();
    }
}