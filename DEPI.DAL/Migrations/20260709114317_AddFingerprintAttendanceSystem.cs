using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DEPI.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFingerprintAttendanceSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Schedules_ScheduleId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_ScheduleId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "TimeIn",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "TimeOut",
                table: "Attendances",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                table: "Attendances",
                newName: "ShiftId");

            migrationBuilder.AddColumn<string>(
                name: "FingerprintId",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInTime",
                table: "Attendances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOutTime",
                table: "Attendances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeSsn",
                table: "Attendances",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_FingerprintId",
                table: "Employees",
                column: "FingerprintId",
                unique: true,
                filter: "[FingerprintId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_EmployeeSsn",
                table: "Attendances",
                column: "EmployeeSsn");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_ShiftId",
                table: "Attendances",
                column: "ShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Employees_EmployeeSsn",
                table: "Attendances",
                column: "EmployeeSsn",
                principalTable: "Employees",
                principalColumn: "EmployeeSsn",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Shifts_ShiftId",
                table: "Attendances",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "ShiftId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Employees_EmployeeSsn",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Shifts_ShiftId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Employees_FingerprintId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_EmployeeSsn",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_ShiftId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "FingerprintId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckOutTime",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "EmployeeSsn",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Attendances");

            migrationBuilder.RenameColumn(
                name: "ShiftId",
                table: "Attendances",
                newName: "ScheduleId");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Attendances",
                newName: "TimeOut");

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeIn",
                table: "Attendances",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_ScheduleId",
                table: "Attendances",
                column: "ScheduleId",
                unique: true,
                filter: "[ScheduleId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Schedules_ScheduleId",
                table: "Attendances",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "ScheduleId");
        }
    }
}
