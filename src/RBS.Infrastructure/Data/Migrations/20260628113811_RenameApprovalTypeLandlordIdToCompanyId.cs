using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameApprovalTypeLandlordIdToCompanyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ApprovalTypes') AND name = 'LandlordId') EXEC sp_rename 'ApprovalTypes.LandlordId', 'CompanyId', 'COLUMN';");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ApprovalLevelConfigs') AND name = 'LandlordId') EXEC sp_rename 'ApprovalLevelConfigs.LandlordId', 'CompanyId', 'COLUMN';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ApprovalTypes') AND name = 'CompanyId') EXEC sp_rename 'ApprovalTypes.CompanyId', 'LandlordId', 'COLUMN';");
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ApprovalLevelConfigs') AND name = 'CompanyId') EXEC sp_rename 'ApprovalLevelConfigs.CompanyId', 'LandlordId', 'COLUMN';");
        }
    }
}
