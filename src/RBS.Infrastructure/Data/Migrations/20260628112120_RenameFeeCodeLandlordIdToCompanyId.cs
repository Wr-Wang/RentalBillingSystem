using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameFeeCodeLandlordIdToCompanyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FeeCodes') AND name = 'LandlordId') EXEC sp_rename 'FeeCodes.LandlordId', 'CompanyId', 'COLUMN';");
            // 重命名唯一索引
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('FeeCodes') AND name = 'IX_FeeCodes_Code_LandlordId') EXEC sp_rename 'FeeCodes.IX_FeeCodes_Code_LandlordId', 'IX_FeeCodes_Code_CompanyId', 'INDEX';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FeeCodes') AND name = 'CompanyId') EXEC sp_rename 'FeeCodes.CompanyId', 'LandlordId', 'COLUMN';");
        }
    }
}
