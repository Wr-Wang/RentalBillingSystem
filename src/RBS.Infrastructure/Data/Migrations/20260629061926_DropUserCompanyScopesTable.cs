using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropUserCompanyScopesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 删除"用户数据权限"菜单及其子菜单
            migrationBuilder.Sql("DELETE FROM [RoleMenus] WHERE [MenuId] IN ('A1111111-1111-1111-1111-111111111164','A1111111-1111-1111-1111-111111111302');");
            migrationBuilder.Sql("DELETE FROM [Menus] WHERE [Id] IN ('A1111111-1111-1111-1111-111111111164','A1111111-1111-1111-1111-111111111302');");

            migrationBuilder.DropTable(
                name: "UserCompanyScopes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 恢复菜单数据
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [Id]='A1111111-1111-1111-1111-111111111164') INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[Path],[Icon],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt]) VALUES ('A1111111-1111-1111-1111-111111111164','用户数据权限','system:userscope','/system/organization/userscope','Unlock','A1111111-1111-1111-1111-111111111016',3,1,'00000000-0000-0000-0000-000000000000',GETDATE());");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM [Menus] WHERE [Id]='A1111111-1111-1111-1111-111111111302') INSERT INTO [Menus] ([Id],[Name],[PermissionCode],[ParentId],[SortOrder],[IsActive],[CreatedBy],[CreatedAt]) VALUES ('A1111111-1111-1111-1111-111111111302','配置权限','system:userscopeconfig','A1111111-1111-1111-1111-111111111164',10,1,'00000000-0000-0000-0000-000000000000',GETDATE());");

            migrationBuilder.CreateTable(
                name: "UserCompanyScopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "公司ID，用户可操作的数据范围"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "用户ID")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCompanyScopes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCompanyScopes_UserId_CompanyId",
                table: "UserCompanyScopes",
                columns: new[] { "UserId", "CompanyId" },
                unique: true);
        }
    }
}
