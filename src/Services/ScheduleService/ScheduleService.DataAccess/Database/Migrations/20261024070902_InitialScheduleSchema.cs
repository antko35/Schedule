using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScheduleService.DataAccess.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialScheduleSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "schedule");

            migrationBuilder.CreateTable(
                name: "calendar_exceptions",
                schema: "schedule",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    holiday_date = table.Column<DateOnly>(type: "date", nullable: false),
                    transfer_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar_exceptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "monthly_schedules",
                schema: "schedule",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    department_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    period = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monthly_schedules", x => x.id);
                    table.CheckConstraint("ck_monthly_schedules_period_first_day", "EXTRACT(DAY FROM period) = 1");
                });

            migrationBuilder.CreateTable(
                name: "schedule_generation_rules",
                schema: "schedule",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    department_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    period = table.Column<DateOnly>(type: "date", nullable: false),
                    target_minutes = table.Column<int>(type: "integer", nullable: false),
                    max_shift_minutes = table.Column<int>(type: "integer", nullable: false),
                    first_shift_start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    second_shift_start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    pattern = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule_generation_rules", x => x.id);
                    table.CheckConstraint("ck_schedule_generation_rules_max_shift_minutes_positive", "max_shift_minutes > 0");
                    table.CheckConstraint("ck_schedule_generation_rules_pattern_defined", "pattern BETWEEN 0 AND 6");
                    table.CheckConstraint("ck_schedule_generation_rules_period_first_day", "EXTRACT(DAY FROM period) = 1");
                    table.CheckConstraint("ck_schedule_generation_rules_target_minutes_positive", "target_minutes > 0");
                });

            migrationBuilder.CreateTable(
                name: "work_shifts",
                schema: "schedule",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    monthly_schedule_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    work_date = table.Column<DateOnly>(type: "date", nullable: false),
                    starts_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_shifts", x => x.id);
                    table.CheckConstraint("ck_work_shifts_valid_interval", "ends_at_utc > starts_at_utc");
                    table.ForeignKey(
                        name: "FK_work_shifts_monthly_schedules_monthly_schedule_id",
                        column: x => x.monthly_schedule_id,
                        principalSchema: "schedule",
                        principalTable: "monthly_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_calendar_exceptions_holiday_date",
                schema: "schedule",
                table: "calendar_exceptions",
                column: "holiday_date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_calendar_exceptions_transfer_date",
                schema: "schedule",
                table: "calendar_exceptions",
                column: "transfer_date",
                unique: true,
                filter: "transfer_date IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_monthly_schedules_department_period",
                schema: "schedule",
                table: "monthly_schedules",
                columns: new[] { "department_id", "period" });

            migrationBuilder.CreateIndex(
                name: "ux_monthly_schedules_user_department_period",
                schema: "schedule",
                table: "monthly_schedules",
                columns: new[] { "user_id", "department_id", "period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_schedule_generation_rules_department_period",
                schema: "schedule",
                table: "schedule_generation_rules",
                columns: new[] { "department_id", "period" });

            migrationBuilder.CreateIndex(
                name: "ux_schedule_generation_rules_user_department_period",
                schema: "schedule",
                table: "schedule_generation_rules",
                columns: new[] { "user_id", "department_id", "period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_work_shifts_schedule_date",
                schema: "schedule",
                table: "work_shifts",
                columns: new[] { "monthly_schedule_id", "work_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calendar_exceptions",
                schema: "schedule");

            migrationBuilder.DropTable(
                name: "schedule_generation_rules",
                schema: "schedule");

            migrationBuilder.DropTable(
                name: "work_shifts",
                schema: "schedule");

            migrationBuilder.DropTable(
                name: "monthly_schedules",
                schema: "schedule");
        }
    }
}
