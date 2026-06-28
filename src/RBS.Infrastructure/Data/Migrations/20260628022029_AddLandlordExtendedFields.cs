using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLandlordExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Landlords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                comment: "通讯地址");

            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "Landlords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "银行账号");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountName",
                table: "Landlords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                comment: "开户名");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Landlords",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                comment: "开户行");

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionRate",
                table: "Landlords",
                type: "decimal(5,2)",
                nullable: true,
                comment: "佣金比例(%)");

            migrationBuilder.AddColumn<string>(
                name: "IdNumber",
                table: "Landlords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "证件号码");

            migrationBuilder.AddColumn<string>(
                name: "IdType",
                table: "Landlords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                comment: "证件类型");

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "Landlords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                comment: "备注");

            migrationBuilder.AddColumn<string>(
                name: "SettlementCycle",
                table: "Landlords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                comment: "结算周期");

            migrationBuilder.AddColumn<int>(
                name: "SettlementDay",
                table: "Landlords",
                type: "int",
                nullable: true,
                comment: "结算日");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Landlords");

            migrationBuilder.DropColumn(
                name: "BankAccount",
                table: "Landlords");

            migrationBuilder.DropColumn(
                name: "BankAccountName",
                table: "Landlords");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Landlords");

            migrationBuilder.DropColumn(
                name: "CommissionRate",
                table: "Landlords");

            migrationBuilder.DropColumn(
                name: "IdNumber",
                table: "Landlords");

            migrationBuilder.DropColumn(
                name: "IdType",
                table: "Landlords");

            migrationBuilder.DropColumn(
                name: "Remark",
                table: "Landlords");

            migrationBuilder.DropColumn(
                name: "SettlementCycle",
                table: "Landlords");

            migrationBuilder.DropColumn(
                name: "SettlementDay",
                table: "Landlords");
        }
    }
}
