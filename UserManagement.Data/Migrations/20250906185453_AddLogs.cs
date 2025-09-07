using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UserManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ActionType = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Logs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Logs",
                columns: new[] { "Id", "ActionType", "Details", "Timestamp", "UserId" },
                values: new object[,]
                {
                    { 1L, "AddUser", "Test text #12", new DateTime(2025, 7, 18, 14, 23, 45, 123, DateTimeKind.Unspecified), 1L },
                    { 2L, "DeleteUser", "Test text #34", new DateTime(2025, 7, 18, 9, 15, 30, 456, DateTimeKind.Unspecified), 2L },
                    { 3L, "UpdateUser", "Test text #56", new DateTime(2025, 7, 18, 22, 5, 12, 789, DateTimeKind.Unspecified), 3L },
                    { 4L, "ViewUser", "Test text #78", new DateTime(2025, 7, 18, 6, 47, 59, 321, DateTimeKind.Unspecified), 4L },
                    { 5L, "DeleteUser", "Test text #90", new DateTime(2025, 7, 18, 17, 33, 10, 654, DateTimeKind.Unspecified), 5L },
                    { 6L, "ViewUser", "Test text #21", new DateTime(2025, 7, 18, 11, 12, 25, 987, DateTimeKind.Unspecified), 6L },
                    { 7L, "UpdateUser", "Test text #43", new DateTime(2025, 7, 18, 19, 44, 38, 210, DateTimeKind.Unspecified), 7L },
                    { 8L, "DeleteUser", "Test text #65", new DateTime(2025, 7, 18, 3, 29, 50, 333, DateTimeKind.Unspecified), 8L },
                    { 9L, "AddUser", "Test text #87", new DateTime(2025, 7, 18, 15, 55, 5, 876, DateTimeKind.Unspecified), 9L },
                    { 10L, "UpdateUser", "Test text #11", new DateTime(2025, 8, 17, 8, 8, 8, 888, DateTimeKind.Unspecified), 10L },
                    { 11L, "ViewUser", "Test text #22", new DateTime(2025, 8, 17, 10, 22, 33, 111, DateTimeKind.Unspecified), 1L },
                    { 12L, "UpdateUser", "Test text #33", new DateTime(2025, 8, 17, 12, 34, 56, 222, DateTimeKind.Unspecified), 2L },
                    { 13L, "AddUser", "Test text #44", new DateTime(2025, 8, 17, 14, 46, 19, 333, DateTimeKind.Unspecified), 3L },
                    { 14L, "AddUser", "Test text #55", new DateTime(2025, 8, 17, 16, 58, 42, 444, DateTimeKind.Unspecified), 4L },
                    { 15L, "AddUser", "Test text #66", new DateTime(2025, 8, 17, 18, 10, 5, 555, DateTimeKind.Unspecified), 5L },
                    { 16L, "UpdateUser", "Test text #77", new DateTime(2025, 8, 17, 20, 21, 28, 666, DateTimeKind.Unspecified), 6L },
                    { 17L, "ViewUser", "Test text #88", new DateTime(2025, 8, 17, 22, 33, 51, 777, DateTimeKind.Unspecified), 7L },
                    { 18L, "AddUser", "Test text #99", new DateTime(2025, 8, 17, 0, 45, 14, 888, DateTimeKind.Unspecified), 8L },
                    { 19L, "DeleteUser", "Test text #10", new DateTime(2025, 8, 17, 2, 56, 37, 999, DateTimeKind.Unspecified), 9L },
                    { 20L, "DeleteUser", "Test text #20", new DateTime(2025, 8, 17, 4, 8, 0, 101, DateTimeKind.Unspecified), 10L },
                    { 21L, "ViewUser", "Test text #30", new DateTime(2025, 8, 27, 6, 19, 23, 202, DateTimeKind.Unspecified), 1L },
                    { 22L, "ViewUser", "Test text #40", new DateTime(2025, 8, 27, 8, 30, 46, 303, DateTimeKind.Unspecified), 2L },
                    { 23L, "UpdateUser", "Test text #50", new DateTime(2025, 8, 27, 10, 42, 9, 404, DateTimeKind.Unspecified), 3L },
                    { 24L, "AddUser", "Test text #60", new DateTime(2025, 8, 27, 12, 53, 32, 505, DateTimeKind.Unspecified), 4L },
                    { 25L, "AddUser", "Test text #70", new DateTime(2025, 8, 27, 14, 4, 55, 606, DateTimeKind.Unspecified), 5L },
                    { 26L, "AddUser", "Test text #80", new DateTime(2025, 8, 27, 16, 16, 18, 707, DateTimeKind.Unspecified), 6L },
                    { 27L, "DeleteUser", "Test text #90", new DateTime(2025, 8, 27, 18, 27, 41, 808, DateTimeKind.Unspecified), 7L },
                    { 28L, "ViewUser", "Test text #15", new DateTime(2025, 8, 27, 20, 39, 4, 909, DateTimeKind.Unspecified), 8L },
                    { 29L, "ViewUser", "Test text #25", new DateTime(2025, 8, 27, 22, 50, 27, 111, DateTimeKind.Unspecified), 9L },
                    { 30L, "ViewUser", "Test text #35", new DateTime(2025, 8, 27, 1, 1, 50, 222, DateTimeKind.Unspecified), 10L },
                    { 31L, "AddUser", "Test text #45", new DateTime(2025, 9, 1, 3, 13, 13, 333, DateTimeKind.Unspecified), 1L },
                    { 32L, "DeleteUser", "Test text #55", new DateTime(2025, 9, 1, 5, 24, 36, 444, DateTimeKind.Unspecified), 2L },
                    { 33L, "UpdateUser", "Test text #65", new DateTime(2025, 9, 1, 7, 35, 59, 555, DateTimeKind.Unspecified), 3L },
                    { 34L, "UpdateUser", "Test text #75", new DateTime(2025, 9, 1, 4, 33, 22, 346, DateTimeKind.Unspecified), 4L },
                    { 35L, "DeleteUser", "Test text #85", new DateTime(2025, 9, 1, 1, 22, 20, 653, DateTimeKind.Unspecified), 5L }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Logs_UserId",
                table: "Logs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");
        }
    }
}
