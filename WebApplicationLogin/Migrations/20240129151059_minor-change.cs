using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationLogin.Migrations
{
    /// <inheritdoc />
    public partial class minorchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "userId",
                table: "PasswordHistories",
                newName: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PasswordHistories",
                newName: "userId");
        }
    }
}
