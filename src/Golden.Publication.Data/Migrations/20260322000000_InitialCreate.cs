#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Golden.Publication.Data.Migrations;

[Migration("20260322000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "publications",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                publication_type = table.Column<string>(type: "text", nullable: false),
                title = table.Column<string>(type: "text", nullable: false),
                description = table.Column<string>(type: "text", nullable: false),
                isbn = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_publications", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "publication_versions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                publication_guid = table.Column<Guid>(type: "uuid", nullable: false),
                version = table.Column<string>(type: "text", nullable: false),
                language = table.Column<string>(type: "text", nullable: false),
                cover_title = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_publication_versions", x => x.id);
                table.ForeignKey(
                    name: "FK_publication_versions_publications_publication_guid",
                    column: x => x.publication_guid,
                    principalTable: "publications",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_publication_versions_publication_guid",
            table: "publication_versions",
            column: "publication_guid");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "publication_versions");
        migrationBuilder.DropTable(name: "publications");
    }
}
