using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameJobScheduleLandlordIdToCompanyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('JobSchedules') AND name = 'LandlordId') EXEC sp_rename 'JobSchedules.LandlordId', 'CompanyId', 'COLUMN';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('JobSchedules') AND name = 'CompanyId') EXEC sp_rename 'JobSchedules.CompanyId', 'LandlordId', 'COLUMN';");
        }
    }
}
