using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DEPI.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Schedules",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Absent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Schedules");
        }
    }
}
