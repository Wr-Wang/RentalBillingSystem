using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameLandlordsAuditToCompaniesAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename table only if Landlords_Audit still exists
            migrationBuilder.Sql("IF OBJECT_ID('Landlords_Audit', 'U') IS NOT NULL EXEC sp_rename 'Landlords_Audit', 'Companies_Audit';");
            // Rename indexes if they exist (errors silently caught)
            migrationBuilder.Sql("IF INDEXPROPERTY(OBJECT_ID('Companies_Audit'), 'IX_Landlords_Audit_EntityId', 'IndexId') IS NOT NULL EXEC sp_rename 'Companies_Audit.IX_Landlords_Audit_EntityId', 'IX_Companies_Audit_EntityId', 'INDEX';");
            migrationBuilder.Sql("IF INDEXPROPERTY(OBJECT_ID('Companies_Audit'), 'IX_Landlords_Audit_ChangedAt', 'IndexId') IS NOT NULL EXEC sp_rename 'Companies_Audit.IX_Landlords_Audit_ChangedAt', 'IX_Companies_Audit_ChangedAt', 'INDEX';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID('Companies_Audit', 'U') IS NOT NULL EXEC sp_rename 'Companies_Audit', 'Landlords_Audit';");
        }
    }
}
