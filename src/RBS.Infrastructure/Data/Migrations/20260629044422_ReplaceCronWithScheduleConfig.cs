using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RBS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCronWithScheduleConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultCronExpression",
                table: "JobTemplates");

            migrationBuilder.DropColumn(
                name: "CronExpression",
                table: "JobSchedules");

            migrationBuilder.AddColumn<int>(
                name: "DefaultDayOfMonth",
                table: "JobTemplates",
                type: "int",
                nullable: true,
                comment: "默认执行日（Monthly类型）");

            migrationBuilder.AddColumn<int>(
                name: "DefaultHour",
                table: "JobTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "默认小时");

            migrationBuilder.AddColumn<int>(
                name: "DefaultMinute",
                table: "JobTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "默认分钟");

            migrationBuilder.AddColumn<string>(
                name: "DefaultScheduleType",
                table: "JobTemplates",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Monthly",
                comment: "默认调度类型");

            migrationBuilder.AddColumn<int>(
                name: "DayOfMonth",
                table: "JobSchedules",
                type: "int",
                nullable: true,
                comment: "执行日 1~31（Monthly 类型使用）");

            migrationBuilder.AddColumn<int>(
                name: "Hour",
                table: "JobSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "执行小时 0~23");

            migrationBuilder.AddColumn<int>(
                name: "Minute",
                table: "JobSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "执行分钟 0~59");

            migrationBuilder.AddColumn<string>(
                name: "ScheduleType",
                table: "JobSchedules",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Monthly",
                comment: "调度类型: Daily/Monthly");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultDayOfMonth",
                table: "JobTemplates");

            migrationBuilder.DropColumn(
                name: "DefaultHour",
                table: "JobTemplates");

            migrationBuilder.DropColumn(
                name: "DefaultMinute",
                table: "JobTemplates");

            migrationBuilder.DropColumn(
                name: "DefaultScheduleType",
                table: "JobTemplates");

            migrationBuilder.DropColumn(
                name: "DayOfMonth",
                table: "JobSchedules");

            migrationBuilder.DropColumn(
                name: "Hour",
                table: "JobSchedules");

            migrationBuilder.DropColumn(
                name: "Minute",
                table: "JobSchedules");

            migrationBuilder.DropColumn(
                name: "ScheduleType",
                table: "JobSchedules");

            migrationBuilder.AddColumn<string>(
                name: "DefaultCronExpression",
                table: "JobTemplates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                comment: "默认 Cron");

            migrationBuilder.AddColumn<string>(
                name: "CronExpression",
                table: "JobSchedules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                comment: "Cron 表达式");
        }
    }
}
