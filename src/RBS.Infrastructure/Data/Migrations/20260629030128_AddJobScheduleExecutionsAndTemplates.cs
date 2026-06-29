using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJobScheduleExecutionsAndTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ScheduledTaskLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                comment: "执行状态（Pending/Running/Completed/Failed/Stale）",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Pending",
                oldComment: "执行状态（Pending/Running/Completed/Failed）");

            migrationBuilder.AddColumn<DateTime>(
                name: "HeartbeatAt",
                table: "ScheduledTaskLogs",
                type: "datetime2",
                nullable: true,
                comment: "心跳时间");

            migrationBuilder.AddColumn<string>(
                name: "TargetMonth",
                table: "ScheduledTaskLogs",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true,
                comment: "目标月份 yyyy-MM");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRunAt",
                table: "JobSchedules",
                type: "datetime2",
                nullable: true,
                comment: "上次执行时间");

            migrationBuilder.AddColumn<string>(
                name: "LastRunStatus",
                table: "JobSchedules",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                comment: "上次执行结果");

            migrationBuilder.AddColumn<string>(
                name: "TemplateCode",
                table: "JobSchedules",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                comment: "创建来源模板代码");

            migrationBuilder.CreateTable(
                name: "JobScheduleExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "所属任务定义ID"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "所属公司ID"),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "排期执行时间"),
                    OriginalDate = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "原始 Cron 时间"),
                    Month = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false, comment: "yyyy-MM"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending", comment: "状态"),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "备注/调整原因"),
                    IsAdjusted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "是否手动调整"),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "是否自定义排期"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "更新时间"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedHostname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedHostname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobScheduleExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobScheduleExecutions_JobSchedules_JobScheduleId",
                        column: x => x.JobScheduleId,
                        principalTable: "JobSchedules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JobTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "模板代码"),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "显示名"),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "短名"),
                    DefaultCronExpression = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "默认 Cron"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "模板说明"),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "前端图标"),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "分类"),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "排序号"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "是否启用"),
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
                    table.PrimaryKey("PK_JobTemplates", x => x.Id);
                });

            // ScheduledTaskLogs 的 LandlordId 列尚未重命名为 CompanyId
            // 执行列重命名后再创建索引
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ScheduledTaskLogs') AND name = 'LandlordId') EXEC sp_rename 'ScheduledTaskLogs.LandlordId', 'CompanyId', 'COLUMN';");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTaskLogs_HeartbeatAt",
                table: "ScheduledTaskLogs",
                column: "HeartbeatAt");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledTaskLogs_JobLock",
                table: "ScheduledTaskLogs",
                columns: new[] { "TaskName", "CompanyId", "TargetMonth" },
                unique: true,
                filter: "[TargetMonth] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Executions_CompanyId",
                table: "JobScheduleExecutions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Executions_JobScheduleId",
                table: "JobScheduleExecutions",
                column: "JobScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Executions_JobScheduleId_Month",
                table: "JobScheduleExecutions",
                columns: new[] { "JobScheduleId", "Month" },
                unique: true,
                filter: "[IsCustom] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Executions_TargetDate",
                table: "JobScheduleExecutions",
                column: "TargetDate");

            migrationBuilder.CreateIndex(
                name: "IX_JobScheduleExecutions_CreatedAt",
                table: "JobScheduleExecutions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobTemplates_Code",
                table: "JobTemplates",
                column: "Code",
                unique: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobScheduleExecutions");

            migrationBuilder.DropTable(
                name: "JobTemplates");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledTaskLogs_HeartbeatAt",
                table: "ScheduledTaskLogs");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledTaskLogs_JobLock",
                table: "ScheduledTaskLogs");

            migrationBuilder.DropColumn(
                name: "HeartbeatAt",
                table: "ScheduledTaskLogs");

            migrationBuilder.DropColumn(
                name: "TargetMonth",
                table: "ScheduledTaskLogs");

            migrationBuilder.DropColumn(
                name: "LastRunAt",
                table: "JobSchedules");

            migrationBuilder.DropColumn(
                name: "LastRunStatus",
                table: "JobSchedules");

            migrationBuilder.DropColumn(
                name: "TemplateCode",
                table: "JobSchedules");

            // Migration Down 时回滚列名
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ScheduledTaskLogs') AND name = 'CompanyId') EXEC sp_rename 'ScheduledTaskLogs.CompanyId', 'LandlordId', 'COLUMN';");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ScheduledTaskLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                comment: "执行状态（Pending/Running/Completed/Failed）",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Pending",
                oldComment: "执行状态（Pending/Running/Completed/Failed/Stale）");
        }
    }
}
