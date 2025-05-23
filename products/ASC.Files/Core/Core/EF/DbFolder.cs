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

namespace ASC.Files.Core.EF;

[Transient]
[OpenSearchType(RelationName = Tables.Folder)]
public class DbFolder : IDbFile, IDbSearch, ISearchItem
{
    public int Id { get; set; }
    public int ParentId { get; set; }

    [Text(Analyzer = "whitespacecustom")]
    [MaxLength(400)]
    public string Title { get; set; }
    public FolderType FolderType { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime CreateOn { get; set; }
    public Guid ModifiedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
    public int TenantId { get; set; }
    public int FoldersCount { get; set; }
    public int FilesCount { get; set; }
    public long Counter { get; set; }
    
    [Ignore]
    public DbRoomSettings Settings { get; set; }
    
    [Ignore]
    public DbTenant Tenant { get; set; }

    [Ignore]
    public string IndexName => Tables.Folder;

    public Expression<Func<ISearchItem, object[]>> GetSearchContentFields(SearchSettingsHelper searchSettings)
    {
        return a => new object[] { Title };
    }
}

public static class DbFolderExtension
{
    public static ModelBuilderWrapper AddDbFolder(this ModelBuilderWrapper modelBuilder)
    {
        modelBuilder.Entity<DbFolder>().Navigation(e => e.Tenant).AutoInclude(false);

        modelBuilder
            .Add(MySqlAddDbFolder, Provider.MySql)
                .Add(PgSqlAddDbFolder, Provider.PostgreSql);

        return modelBuilder;
    }

    public static void MySqlAddDbFolder(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbFolder>(entity =>
        {
            entity.ToTable("files_folder")
                .HasCharSet("utf8");

            entity.Ignore(r => r.IndexName);

            entity.HasIndex(e => e.ModifiedOn)
                .HasDatabaseName("modified_on");

            entity.HasIndex(e => new { e.TenantId, e.ParentId })
                .HasDatabaseName("parent_id");

            entity.HasIndex(e => new { e.TenantId, e.ParentId, e.Title })
                .HasDatabaseName("tenant_id_parent_id_title");

            entity.HasIndex(e => new { e.TenantId, e.ParentId, e.ModifiedOn })
                .HasDatabaseName("tenant_id_parent_id_modified_on");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CreateBy)
                .IsRequired()
                .HasColumnName("create_by")
                .HasColumnType("char(38)")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.CreateOn)
                .HasColumnName("create_on")
                .HasColumnType("datetime");

            entity.Property(e => e.FilesCount)
                .HasColumnName("filesCount")
                .HasDefaultValueSql("'0'");

            entity.Property(e => e.FolderType)
                .HasColumnName("folder_type")
                .HasDefaultValueSql("'0'");

            entity.Property(e => e.FoldersCount)
                .HasColumnName("foldersCount")
                .HasDefaultValueSql("'0'");

            entity.Property(e => e.ModifiedBy)
                .IsRequired()
                .HasColumnName("modified_by")
                .HasColumnType("char(38)")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.ModifiedOn)
                .HasColumnName("modified_on")
                .HasColumnType("datetime");

            entity.Property(e => e.ParentId)
                .HasColumnName("parent_id")
                .HasDefaultValueSql("'0'");

            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasColumnName("title")
                .HasColumnType("varchar")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Counter)
                .HasColumnName("counter")
                .HasDefaultValueSql("'0'");
        });
    }
    public static void PgSqlAddDbFolder(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbFolder>(entity =>
        {
            entity.ToTable("files_folder");

            entity.HasKey(e => e.Id)
                .HasName("PK_files_folder");
            
            entity.Ignore(r => r.IndexName);

            entity.HasIndex(e => e.ModifiedOn)
                .HasDatabaseName("IX_files_folder_modified_on");

            entity.HasIndex(e => new { e.TenantId, e.ParentId })
                .HasDatabaseName("parent_id");

            entity.HasIndex(e => new { e.TenantId, e.ParentId, e.Title })
                .HasDatabaseName("tenant_id_parent_id_title");

            entity.HasIndex(e => new { e.TenantId, e.ParentId, e.ModifiedOn })
                .HasDatabaseName("tenant_id_parent_id_modified_on");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CreateBy)
                .IsRequired()
                .HasColumnName("create_by")
                .HasColumnType("uuid");

            entity.Property(e => e.CreateOn)
                .HasColumnName("create_on")
                .HasColumnType("timestamptz");

            entity.Property(e => e.FilesCount)
                .HasColumnName("filesCount")
                .HasDefaultValueSql("0");

            entity.Property(e => e.FolderType)
                .HasColumnName("folder_type")
                .HasDefaultValueSql("0");

            entity.Property(e => e.FoldersCount)
                .HasColumnName("foldersCount")
                .HasDefaultValueSql("0");

            entity.Property(e => e.ModifiedBy)
                .IsRequired()
                .HasColumnName("modified_by")
                .HasColumnType("uuid");

            entity.Property(e => e.ModifiedOn)
                .HasColumnName("modified_on")
                .HasColumnType("timestamptz");

            entity.Property(e => e.ParentId)
                .HasColumnName("parent_id")
                .HasDefaultValueSql("0");

            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasColumnName("title")
                .HasColumnType("character varying");

            entity.Property(e => e.Counter)
                .HasColumnName("counter")
                .HasDefaultValueSql("0");
        });
    }
}
