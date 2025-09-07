using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UserManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Forename = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "DateOfBirth", "Email", "Forename", "IsActive", "Surname" },
                values: new object[,]
                {
                    { 1L, null, "ploew@example.com", "Peter", true, "Loew" },
                    { 2L, null, "bfgates@example.com", "Benjamin Franklin", true, "Gates" },
                    { 3L, null, "ctroy@example.com", "Castor", false, "Troy" },
                    { 4L, null, "mraines@example.com", "Memphis", true, "Raines" },
                    { 5L, null, "sgodspeed@example.com", "Stanley", true, "Goodspeed" },
                    { 6L, null, "himcdunnough@example.com", "H.I.", true, "McDunnough" },
                    { 7L, null, "cpoe@example.com", "Cameron", false, "Poe" },
                    { 8L, null, "emalus@example.com", "Edward", false, "Malus" },
                    { 9L, null, "dmacready@example.com", "Damon", false, "Macready" },
                    { 10L, null, "jblaze@example.com", "Johnny", true, "Blaze" },
                    { 11L, null, "rfeld@example.com", "Robin", true, "Feld" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");
        }
    }
}
