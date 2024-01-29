using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationLogin.Migrations
{
    /// <inheritdoc />
    public partial class addguid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GUID",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GUID",
                table: "AspNetUsers");
        }
    }
}
