using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    public partial class AddLateFeeConfigFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 统一列名：LandlordId → CompanyId
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LateFeeConfigs') AND name = 'LandlordId') EXEC sp_rename 'LateFeeConfigs.LandlordId', 'CompanyId', 'COLUMN';");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxRate", table: "LateFeeConfigs",
                type: "decimal(5,2)", precision: 5, scale: 2, nullable: true,
                comment: "滞纳金上限百分比",
                oldClrType: typeof(decimal), oldType: "decimal(5,2)",
                oldPrecision: 5, oldScale: 2, oldNullable: true,
                oldComment: "滞纳金上限（百分比）");

            migrationBuilder.AlterColumn<decimal>(
                name: "DailyRate", table: "LateFeeConfigs",
                type: "decimal(7,5)", precision: 7, scale: 5, nullable: false,
                comment: "日利率（如 0.0005 表示日息万分之五）",
                oldClrType: typeof(decimal), oldType: "decimal(5,4)",
                oldPrecision: 5, oldScale: 4,
                oldComment: "日利率（如 0.0005 表示日息万分之五）");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveDate", table: "LateFeeConfigs", type: "date",
                nullable: false, defaultValue: new DateOnly(1, 1, 1),
                comment: "生效日期");

            migrationBuilder.AddColumn<decimal>(
                name: "MinAmount", table: "LateFeeConfigs",
                type: "decimal(18,2)", precision: 18, scale: 2, nullable: true,
                comment: "最低滞纳金金额");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 回滚列名
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('LateFeeConfigs') AND name = 'CompanyId') EXEC sp_rename 'LateFeeConfigs.CompanyId', 'LandlordId', 'COLUMN';");

            migrationBuilder.DropColumn(name: "EffectiveDate", table: "LateFeeConfigs");
            migrationBuilder.DropColumn(name: "MinAmount", table: "LateFeeConfigs");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxRate", table: "LateFeeConfigs",
                type: "decimal(5,2)", precision: 5, scale: 2, nullable: true,
                comment: "滞纳金上限（百分比）",
                oldClrType: typeof(decimal), oldType: "decimal(5,2)",
                oldPrecision: 5, oldScale: 2, oldNullable: true,
                oldComment: "滞纳金上限百分比");

            migrationBuilder.AlterColumn<decimal>(
                name: "DailyRate", table: "LateFeeConfigs",
                type: "decimal(5,4)", precision: 5, scale: 4, nullable: false,
                comment: "日利率（如 0.0005 表示日息万分之五）",
                oldClrType: typeof(decimal), oldType: "decimal(7,5)",
                oldPrecision: 7, oldScale: 5,
                oldComment: "日利率（如 0.0005 表示日息万分之五）");
        }
    }
}
