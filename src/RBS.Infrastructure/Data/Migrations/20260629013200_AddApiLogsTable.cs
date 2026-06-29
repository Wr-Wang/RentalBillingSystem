using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApiLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserDisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "用户显示名"),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "HTTP 方法"),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "请求路径"),
                    QueryString = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true, comment: "查询参数"),
                    RequestBody = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "请求体"),
                    StatusCode = table.Column<int>(type: "int", nullable: false, comment: "HTTP 状态码"),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "响应体"),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false, comment: "耗时(ms)"),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "客户端IP"),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "用户代理"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "创建时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_CreatedAt",
                table: "ApiLogs",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_Path",
                table: "ApiLogs",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_UserId",
                table: "ApiLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiLogs");
        }
    }
}
