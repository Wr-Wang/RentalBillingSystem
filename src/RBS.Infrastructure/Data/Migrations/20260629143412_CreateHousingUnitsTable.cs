using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    public partial class CreateHousingUnitsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 创建 HousingUnits 表
            migrationBuilder.CreateTable(
                name: "HousingUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "座楼名称"),
                    BuildingCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "座楼编码"),
                    BuildingAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "地址"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "所属公司"),
                    FloorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "楼层名称"),
                    FloorSortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "楼层排序"),
                    UnitNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "房源编号"),
                    FullCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "完整编码"),
                    RoomTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Area = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, comment: "面积"),
                    Orientation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, comment: "朝向"),
                    BaseRentAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, comment: "基础月租金"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Vacant", comment: "状态"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedHostname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedHostname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_HousingUnits", x => x.Id));

            // 迁移已有数据：Rooms + Floors + Buildings → HousingUnits
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'Rooms' AND type = 'U')
AND NOT EXISTS (SELECT 1 FROM HousingUnits)
BEGIN
    INSERT INTO HousingUnits (Id, BuildingName, BuildingCode, BuildingAddress, CompanyId, FloorName, FloorSortOrder, UnitNo, FullCode, RoomTypeId, Area, Orientation, BaseRentAmount, Status, CreatedBy, CreatedAt, CreatedIp, CreatedHostname, UpdatedBy, UpdatedAt, UpdatedIp, UpdatedHostname)
    SELECT
        r.Id,
        ISNULL(b.Name, N'未知座楼'),
        b.Code,
        b.Address,
        b.CompanyId,
        ISNULL(f.Name, N'未知楼层'),
        ISNULL(f.SortOrder, 0),
        r.RoomNo,
        r.FullCode,
        r.RoomTypeId,
        r.Area,
        r.Orientation,
        r.BaseRentAmount,
        r.Status,
        r.CreatedBy, r.CreatedAt, r.CreatedIp, r.CreatedHostname,
        r.UpdatedBy, r.UpdatedAt, r.UpdatedIp, r.UpdatedHostname
    FROM Rooms r
    LEFT JOIN Floors f ON f.Id = r.FloorId
    LEFT JOIN Buildings b ON b.Id = f.BuildingId;
END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "HousingUnits");
        }
    }
}
