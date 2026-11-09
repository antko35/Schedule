using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagementService.DataAccess.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialUserManagementSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "user_management");

            migrationBuilder.CreateTable(
                name: "clinics",
                schema: "user_management",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ClinicName = table.Column<string>(type: "text", nullable: false),
                    ClinicDescription = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                schema: "user_management",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    DepartmentName = table.Column<string>(type: "text", nullable: true),
                    ClinicId = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "user_management",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Patronymic = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_jobs",
                schema: "user_management",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    UserId = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    DepartmentId = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Role = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_jobs", x => x.Id);
                    table.ForeignKey(
                        name: "fk_user_jobs_departments",
                        column: x => x.DepartmentId,
                        principalSchema: "user_management",
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_jobs_users",
                        column: x => x.UserId,
                        principalSchema: "user_management",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_departments_clinic_name",
                schema: "user_management",
                table: "departments",
                columns: new[] { "ClinicId", "DepartmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_jobs_DepartmentId",
                schema: "user_management",
                table: "user_jobs",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "ux_user_jobs_user_department",
                schema: "user_management",
                table: "user_jobs",
                columns: new[] { "UserId", "DepartmentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clinics",
                schema: "user_management");

            migrationBuilder.DropTable(
                name: "user_jobs",
                schema: "user_management");

            migrationBuilder.DropTable(
                name: "departments",
                schema: "user_management");

            migrationBuilder.DropTable(
                name: "users",
                schema: "user_management");
        }
    }
}
