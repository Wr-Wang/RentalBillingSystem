using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDeadTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingFloorLevelConfigs");

            migrationBuilder.DropTable(
                name: "RoomFeeDefaults");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingFloorLevelConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FloorLevelBandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingFloorLevelConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomFeeDefaults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, comment: "默认金额"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "所属公司ID"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedHostname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FeeCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "费用项目ID"),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "房间ID"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedHostname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomFeeDefaults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomFeeDefaults_CreatedAt",
                table: "RoomFeeDefaults",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RoomFeeDefaults_RoomId_FeeCodeId",
                table: "RoomFeeDefaults",
                columns: new[] { "RoomId", "FeeCodeId" },
                unique: true);
        }
    }
}
