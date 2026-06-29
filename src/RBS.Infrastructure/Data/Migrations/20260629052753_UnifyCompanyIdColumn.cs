using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    public partial class UnifyCompanyIdColumn : Migration
    {
        private readonly string[] _tables = {
            "AccountingSubjects", "ApprovalRequests", "BuildingFloorLevelConfigs",
            "Buildings", "CollectionStages", "Contracts",
            "FeeCodeTemplates", "MeterEstimationConfigs", "PaymentChannels",
            "Receipts", "RoomFeeDefaults", "RoomPricingStandards",
            "TaxRateConfigs", "Tenants"
        };

        protected override void Up(MigrationBuilder mb)
        {
            foreach (var table in _tables)
            {
                mb.Sql($@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('{table}') AND name = 'LandlordId')
EXEC sp_rename '[{table}].LandlordId', 'CompanyId', 'COLUMN';");
            }

            // Users 表：HomeLandlordId → HomeCompanyId
            mb.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'HomeLandlordId')
EXEC sp_rename 'Users.HomeLandlordId', 'HomeCompanyId', 'COLUMN';");

            // UserLandlordScopes 表重命名 + 列重命名
            mb.Sql("IF OBJECT_ID('UserLandlordScopes', 'U') IS NOT NULL EXEC sp_rename 'UserLandlordScopes', 'UserCompanyScopes';");
            mb.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserCompanyScopes') AND name = 'LandlordId')
EXEC sp_rename 'UserCompanyScopes.LandlordId', 'CompanyId', 'COLUMN';");
        }

        protected override void Down(MigrationBuilder mb) { }
    }
}
