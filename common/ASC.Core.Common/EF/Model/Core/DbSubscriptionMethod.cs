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

namespace ASC.Core.Common.EF;

public class DbSubscriptionMethod : BaseEntity
{
    public int TenantId { get; set; }
    [MaxLength(38)]
    public string Source { get; set; }
    [MaxLength(128)]
    public string Action { get; set; }
    [MaxLength(38)]
    public string Recipient { get; set; }
    [MaxLength(1024)]
    public string Sender { get; set; }

    public DbTenant Tenant { get; set; }

    public override object[] GetKeys()
    {
        return [TenantId, Source, Action, Recipient];
    }
}

public static class SubscriptionMethodExtension
{
    public static ModelBuilderWrapper AddSubscriptionMethod(this ModelBuilderWrapper modelBuilder)
    {
        modelBuilder.Entity<DbSubscriptionMethod>().Navigation(e => e.Tenant).AutoInclude(false);

        modelBuilder
            .Add(MySqlAddSubscriptionMethod, Provider.MySql)
            .Add(PgSqlAddSubscriptionMethod, Provider.PostgreSql)
            .HasData(
            new DbSubscriptionMethod { Source = "asc.web.studio", Action = "send_whats_new", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6504977c-75af-4691-9099-084d3ddeea04", Action = "new feed", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6a598c74-91ae-437d-a5f4-ad339bd11bb2", Action = "new post", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "853b6eb9-73ee-438d-9b09-8ffeedf36234", Action = "new topic in forum", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "9d51954f-db9b-4aed-94e3-ed70b914e101", Action = "new photo uploaded", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "28b10049-dd20-4f54-b986-873bc14ccfc7", Action = "new bookmark created", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "742cf945-cbbc-4a57-82d6-1600a12cf8ca", Action = "new wiki page", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "37620ae5-c40b-45ce-855a-39dd7d76a1fa", Action = "BirthdayReminder", Recipient = "abef62db-11a8-4673-9d32-ef1d8af19dc0", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6fe286a4-479e-4c25-a8d9-0156e332b0c0", Action = "sharedocument", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6fe286a4-479e-4c25-a8d9-0156e332b0c0", Action = "sharefolder", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6fe286a4-479e-4c25-a8d9-0156e332b0c0", Action = "updatedocument", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6045b68c-2c2e-42db-9e53-c272e814c4ad", Action = "invitetoproject", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6045b68c-2c2e-42db-9e53-c272e814c4ad", Action = "milestonedeadline", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6045b68c-2c2e-42db-9e53-c272e814c4ad", Action = "newcommentformessage", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6045b68c-2c2e-42db-9e53-c272e814c4ad", Action = "newcommentformilestone", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6045b68c-2c2e-42db-9e53-c272e814c4ad", Action = "newcommentfortask", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6045b68c-2c2e-42db-9e53-c272e814c4ad", Action = "projectcreaterequest", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6045b68c-2c2e-42db-9e53-c272e814c4ad", Action = "projecteditrequest", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6045b68c-2c2e-42db-9e53-c272e814c4ad", Action = "removefromproject", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6045b68c-2c2e-42db-9e53-c272e814c4ad", Action = "responsibleforproject", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6045b68c-2c2e-42db-9e53-c272e814c4ad", Action = "responsiblefortask", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "6045b68c-2c2e-42db-9e53-c272e814c4ad", Action = "taskclosed", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "40650da3-f7c1-424c-8c89-b9c115472e08", Action = "calendar_sharing", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "40650da3-f7c1-424c-8c89-b9c115472e08", Action = "event_alert", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "asc.web.studio", Action = "admin_notify", Recipient = "cd84e66b-b803-40fc-99f9-b2969a54a1de", Sender = "email.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "13ff36fb-0272-4887-b416-74f52b0d0b02", Action = "SetAccess", Recipient = "abef62db-11a8-4673-9d32-ef1d8af19dc0", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "13ff36fb-0272-4887-b416-74f52b0d0b02", Action = "ResponsibleForTask", Recipient = "abef62db-11a8-4673-9d32-ef1d8af19dc0", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "13ff36fb-0272-4887-b416-74f52b0d0b02", Action = "AddRelationshipEvent", Recipient = "abef62db-11a8-4673-9d32-ef1d8af19dc0", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "13ff36fb-0272-4887-b416-74f52b0d0b02", Action = "ExportCompleted", Recipient = "abef62db-11a8-4673-9d32-ef1d8af19dc0", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "13ff36fb-0272-4887-b416-74f52b0d0b02", Action = "CreateNewContact", Recipient = "abef62db-11a8-4673-9d32-ef1d8af19dc0", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "13ff36fb-0272-4887-b416-74f52b0d0b02", Action = "ResponsibleForOpportunity", Recipient = "abef62db-11a8-4673-9d32-ef1d8af19dc0", Sender = "email.sender|messanger.sender", TenantId = -1 },
            new DbSubscriptionMethod { Source = "asc.web.studio", Action = "periodic_notify", Recipient = "c5cc67d1-c3e8-43c0-a3ad-3928ae3e5b5e", Sender = "email.sender", TenantId = -1 }
            );

        return modelBuilder;
    }

    public static void MySqlAddSubscriptionMethod(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbSubscriptionMethod>(entity =>
        {
            entity.HasKey(e => new { e.TenantId, e.Source, e.Action, e.Recipient })
                .HasName("PRIMARY");

            entity.ToTable("core_subscriptionmethod")
                .HasCharSet("utf8");

            entity.Property(e => e.TenantId).HasColumnName("tenant");

            entity.Property(e => e.Source)
                .HasColumnName("source")
                .HasColumnType("varchar")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Action)
                .HasColumnName("action")
                .HasColumnType("varchar")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Recipient)
                .HasColumnName("recipient")
                .HasColumnType("varchar")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            entity.Property(e => e.Sender)
                .IsRequired()
                .HasColumnName("sender")
                .HasColumnType("varchar")
                .HasCharSet("utf8")
                .UseCollation("utf8_general_ci");
        });
    }
    public static void PgSqlAddSubscriptionMethod(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbSubscriptionMethod>(entity =>
        {
            entity.HasKey(e => new { e.TenantId, e.Source, e.Action, e.Recipient });

            entity.ToTable("core_subscriptionmethod");

            entity.Property(e => e.TenantId).HasColumnName("tenant");

            entity.Property(e => e.Source)
                .HasColumnName("source")
                .HasColumnType("varchar");

            entity.Property(e => e.Action)
                .HasColumnName("action")
                .HasColumnType("varchar");

            entity.Property(e => e.Recipient)
                .HasColumnName("recipient")
                .HasColumnType("varchar");

            entity.Property(e => e.Sender)
                .IsRequired()
                .HasColumnName("sender")
                .HasColumnType("varchar");
        });
    }
}
