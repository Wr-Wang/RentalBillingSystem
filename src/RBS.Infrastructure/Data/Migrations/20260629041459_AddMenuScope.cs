using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DefaultCompanyId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true,
                comment: "默认公司ID（用于写入操作）");

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "Menus",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Company",
                comment: "可见范围: Company(公司级,数据隔离) / System(仅超管)");

            // 设置超管专属菜单的 Scope = System
            migrationBuilder.Sql("UPDATE [Menus] SET [Scope] = 'System' WHERE [Path] IN ('/system/companies','/system/organization/user-scope','/system/logs','/system/apilogs','/system/menus')");
            // 补漏：通过 PermissionCode 匹配
            migrationBuilder.Sql("UPDATE [Menus] SET [Scope] = 'System' WHERE [PermissionCode] LIKE 'company:%' OR [PermissionCode] LIKE 'user:scope' OR [PermissionCode] = 'admin:company'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultCompanyId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "Menus");
        }
    }
}
