using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateSystemLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "日志级别"),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true, comment: "日志消息"),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "异常堆栈"),
                    Source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "来源"),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "请求路径"),
                    Method = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, comment: "请求方法"),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "客户端IP"),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "用户代理"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()", comment: "创建时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_CreatedAt",
                table: "SystemLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_Level",
                table: "SystemLogs",
                column: "Level");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemLogs");
        }
    }
}
