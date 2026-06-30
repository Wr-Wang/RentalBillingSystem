using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropBuildingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Floors 表可能还存在旧 FK，需先删除
            if (migrationBuilder.ActiveProvider?.Contains("SqlServer") == true)
                migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Floors_Buildings_BuildingId') ALTER TABLE [Floors] DROP CONSTRAINT [FK_Floors_Buildings_BuildingId];");
            migrationBuilder.DropTable(name: "Buildings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "楼宇地址"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "楼宇编码"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "所属公司ID"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedHostname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否启用"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "楼宇名称"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedHostname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_CompanyId",
                table: "Buildings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_CreatedAt",
                table: "Buildings",
                column: "CreatedAt");
        }
    }
}
