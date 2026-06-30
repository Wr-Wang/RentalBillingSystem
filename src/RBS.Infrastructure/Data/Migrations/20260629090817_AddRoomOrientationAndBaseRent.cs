using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomOrientationAndBaseRent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BaseRentAmount",
                table: "Rooms",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                comment: "基础月租金");

            migrationBuilder.AddColumn<string>(
                name: "Orientation",
                table: "Rooms",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                comment: "朝向（东/南/西/北/南北通透）");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseRentAmount",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Orientation",
                table: "Rooms");
        }
    }
}
