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

public class DbFilesThirdpartyAccount : BaseEntity, IDbFile, IDbSearch
{
    public int Id { get; set; }
    [MaxLength(50)]
    public string Provider { get; set; }
    [MaxLength(400)]
    public string Title { get; set; }
    [MaxLength(100)]
    public string UserName { get; set; }
    [MaxLength(512)]
    public string Password { get; set; }
    public string Token { get; set; }
    public Guid UserId { get; set; }
    public FolderType FolderType { get; set; }
    public FolderType RoomType { get; set; }
    public DateTime CreateOn { get; set; }
    public string Url { get; set; }
    public int TenantId { get; set; }
    public string FolderId { get; set; }
    public bool Private { get; set; }
    public bool HasLogo { get; set; }
    [MaxLength(6)]
    public string Color { get; set; }
    public DateTime ModifiedOn { get; set; }

    public DbTenant Tenant { get; set; }

    public override object[] GetKeys()
    {
        return [Id];
    }
}

public static class DbFilesThirdpartyAccountExtension
{
    public static ModelBuilderWrapper AddDbFilesThirdpartyAccount(this ModelBuilderWrapper modelBuilder)
    {
        modelBuilder.Entity<DbFilesThirdpartyAccount>().Navigation(e => e.Tenant).AutoInclude(false);

        modelBuilder
            .Add(MySqlAddDbFilesThirdpartyAccount, Provider.MySql)
            .Add(PgSqlAddDbFilesThirdpartyAccount, Provider.PostgreSql);

        return modelBuilder;
    }

    public static void MySqlAddDbFilesThirdpartyAccount(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbFilesThirdpartyAccount>(entity =>
        {
            entity.ToTable("files_thirdparty_account")
                .HasCharSet("utf8");

            entity.HasIndex(e => e.TenantId).HasDatabaseName("tenant_id");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CreateOn)
                .HasColumnName("create_on")
                .HasColumnType("datetime");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasColumnName("customer_title")
                .HasColumnType("varchar")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.FolderType)
                .HasColumnName("folder_type")
                .HasDefaultValueSql("'0'");
            entity.Property(e => e.RoomType).HasColumnName("room_type");

            entity.Property(e => e.Password)
                .IsRequired()
                .HasColumnName("password")
                .HasColumnType("varchar")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Provider)
                .IsRequired()
                .HasColumnName("provider")
                .HasColumnType("varchar")
                .HasDefaultValueSql("'0'")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.Property(e => e.Token)
                .HasColumnName("token")
                .HasColumnType("text")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Url)
                .HasColumnName("url")
                .HasColumnType("text")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasColumnName("user_id")
                .HasColumnType("varchar(38)")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.UserName)
                .IsRequired()
                .HasColumnName("user_name")
                .HasColumnType("varchar")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.FolderId)
                .HasColumnName("folder_id")
                .HasColumnType("text")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Private).HasColumnName("private");

            entity.Property(e => e.HasLogo).HasColumnName("has_logo");
            
            entity.Property(e => e.Color)
                .HasColumnName("color")
                .HasColumnType("char")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.ModifiedOn)
                .HasColumnName("modified_on")
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
    
    public static void PgSqlAddDbFilesThirdpartyAccount(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbFilesThirdpartyAccount>(entity =>
        {
            entity.ToTable("files_thirdparty_account");

            entity.HasIndex(e => e.TenantId).HasDatabaseName("IX_files_thirdparty_account_tenant_id");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.CreateOn)
                .HasColumnName("create_on")
                .HasColumnType("timestamptz");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasColumnName("customer_title");

            entity.Property(e => e.FolderType).HasColumnName("folder_type");

            entity.Property(e => e.RoomType).HasColumnName("room_type");

            entity.Property(e => e.Password)
                .IsRequired()
                .HasColumnName("password");

            entity.Property(e => e.Provider)
                .IsRequired()
                .HasColumnName("provider");

            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.Property(e => e.Token).HasColumnName("token");

            entity.Property(e => e.Url).HasColumnName("url");

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasColumnName("user_id");

            entity.Property(e => e.UserName)
                .IsRequired()
                .HasColumnName("user_name");

            entity.Property(e => e.FolderId).HasColumnName("folder_id");

            entity.Property(e => e.Private).HasColumnName("private");

            entity.Property(e => e.HasLogo).HasColumnName("has_logo");

            entity.Property(e => e.Color)
                .HasColumnName("color")
                .HasColumnType("char(6)");

            entity.Property(e => e.ModifiedOn)
                .HasColumnName("modified_on")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
        
    }
}
