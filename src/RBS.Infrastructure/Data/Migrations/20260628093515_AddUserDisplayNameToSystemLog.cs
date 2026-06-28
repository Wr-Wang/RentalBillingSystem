using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDisplayNameToSystemLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserDisplayName",
                table: "SystemLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                comment: "用户显示名称");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserDisplayName",
                table: "SystemLogs");
        }
    }
}
