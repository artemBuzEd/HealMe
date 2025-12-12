using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Appointments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdsToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DoctorUserId",
                schema: "appointments",
                table: "Appointment",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PatientUserId",
                schema: "appointments",
                table: "Appointment",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoctorUserId",
                schema: "appointments",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "PatientUserId",
                schema: "appointments",
                table: "Appointment");
        }
    }
}
