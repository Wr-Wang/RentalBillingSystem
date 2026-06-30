using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImportTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    ValidRows = table.Column<int>(type: "int", nullable: false),
                    FailedRows = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApprovalRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SummaryJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedHostname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedHostname = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportBatchItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowIndex = table.Column<int>(type: "int", nullable: false),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FixSuggestion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Area = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BaseRentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BuildingAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BuildingCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BuildingName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FloorName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FloorSortOrder = table.Column<int>(type: "int", nullable: true),
                    FullCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Orientation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PriceWarning = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RoomTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RoomTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnitNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedHostname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedHostname = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBatchItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportBatchItems_ImportBatches_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalTable: "ImportBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_CompanyId_Status",
                table: "ImportBatches",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_ImportType_Status",
                table: "ImportBatches",
                columns: new[] { "ImportType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatchItems_BuildingName_FloorName_UnitNo",
                table: "ImportBatchItems",
                columns: new[] { "BuildingName", "FloorName", "UnitNo" },
                filter: "[ImportType] = 'HousingUnit' AND [BuildingName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatchItems_ImportBatchId_IsValid",
                table: "ImportBatchItems",
                columns: new[] { "ImportBatchId", "IsValid" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportBatchItems");

            migrationBuilder.DropTable(
                name: "ImportBatches");
        }
    }
}
