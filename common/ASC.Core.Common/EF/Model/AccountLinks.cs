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

namespace ASC.Core.Common.EF.Model;

public class AccountLinks : BaseEntity
{
    [MaxLength(200)]
    public string Id { get; set; }
    [MaxLength(200)]
    public string UId { get; set; }
    [MaxLength(60)]
    public string Provider { get; set; }
    public string Profile { get; set; }
    public DateTime Linked { get; set; }

    public override object[] GetKeys()
    {
        return [Id, UId];
    }
}

public static class AccountLinksExtension
{
    public static ModelBuilderWrapper AddAccountLinks(this ModelBuilderWrapper modelBuilder)
    {
        modelBuilder
            .Add(MySqlAddAccountLinks, Provider.MySql)
            .Add(PgSqlAddAccountLinks, Provider.PostgreSql);

        return modelBuilder;
    }

    public static void MySqlAddAccountLinks(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountLinks>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.UId })
                .HasName("PRIMARY");

            entity.ToTable("account_links")
                .HasCharSet("utf8");

            entity.HasIndex(e => e.UId)
                .HasDatabaseName("uid");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("varchar")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.UId)
                .HasColumnName("uid")
                .HasColumnType("varchar")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Linked)
                .HasColumnName("linked")
                .HasColumnType("datetime");

            entity.Property(e => e.Profile)
                .IsRequired()
                .HasColumnName("profile")
                .HasColumnType("text")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Provider)
                .HasColumnName("provider")
                .HasColumnType("char")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");
        });
    }
    public static void PgSqlAddAccountLinks(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountLinks>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.UId })
                .HasName("PK_account_links");

            entity.ToTable("account_links");

            entity.HasIndex(e => e.UId)
                .HasDatabaseName("ix_account_links_uid");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("varchar")
                .HasMaxLength(200);

            entity.Property(e => e.UId)
                .HasColumnName("uid")
                .HasColumnType("varchar")
                .HasMaxLength(200);

            entity.Property(e => e.Linked)
                .HasColumnName("linked")
                .HasColumnType("timestamp with time zone");

            entity.Property(e => e.Profile)
                .IsRequired()
                .HasColumnName("profile")
                .HasColumnType("text");

            entity.Property(e => e.Provider)
                .HasColumnName("provider")
                .HasColumnType("char")
                .HasMaxLength(60);
        });
        
    }
}
