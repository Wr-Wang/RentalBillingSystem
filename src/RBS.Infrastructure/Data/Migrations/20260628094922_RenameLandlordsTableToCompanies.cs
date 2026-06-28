using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameLandlordsTableToCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 检查旧表存在才重命名（避免重复执行报错）
            migrationBuilder.Sql("IF OBJECT_ID('Landlords', 'U') IS NOT NULL EXEC sp_rename 'Landlords', 'Companies';");
            // 重命名相关索引（Landlords 表原有索引）
            migrationBuilder.Sql("IF INDEXPROPERTY(OBJECT_ID('Companies'), 'IX_Landlords_LandlordId', 'IndexId') IS NOT NULL EXEC sp_rename 'Companies.IX_Landlords_LandlordId', 'IX_Companies_CompanyId', 'INDEX';");
            // 重命名用户表中外键列（HomeLandlordId 已重命名为 HomeCompanyId）
            // IX_Users_HomeLandlordId 索引已随列重命名更新
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID('Companies', 'U') IS NOT NULL EXEC sp_rename 'Companies', 'Landlords';");
        }
    }
}
