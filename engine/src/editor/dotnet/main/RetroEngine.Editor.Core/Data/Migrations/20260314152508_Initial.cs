using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetroEngine.Editor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recent_projects",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    path = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    last_opened = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recent_projects", x => x.id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_recent_projects_last_opened",
                table: "recent_projects",
                column: "last_opened"
            );

            migrationBuilder.CreateIndex(
                name: "ix_recent_projects_path",
                table: "recent_projects",
                column: "path",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "recent_projects");
        }
    }
}
