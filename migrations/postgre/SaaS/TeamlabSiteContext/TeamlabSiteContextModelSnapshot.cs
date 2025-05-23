// <auto-generated />
using ASC.Core.Common.EF.Teamlabsite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ASC.Migrations.PostgreSql.SaaS.Migrations
{
    [DbContext(typeof(TeamlabSiteContext))]
    partial class TeamlabSiteContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("ASC.Core.Common.EF.Teamlabsite.Model.DbCache", b =>
                {
                    b.Property<string>("TenantAlias")
                        .HasMaxLength(100)
                        .HasColumnType("character varying")
                        .HasColumnName("tenant_alias");

                    b.HasKey("TenantAlias")
                        .HasName("PRIMARY");

                    b.ToTable("tenants_cache", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
