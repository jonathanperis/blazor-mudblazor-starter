using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebClient.Features.Notebook.Migrations
{
    /// <inheritdoc />
    public partial class InitialNotebook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Workspace = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Version = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_Workspace",
                table: "Notes",
                column: "Workspace");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notes");
        }
    }
}
