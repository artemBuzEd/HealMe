using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Doctors.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AppointmentId",
                schema: "doctors",
                table: "DoctorReview",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PatientFirstName",
                schema: "doctors",
                table: "DoctorReview",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PatientLastName",
                schema: "doctors",
                table: "DoctorReview",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "doctors",
                table: "DoctorReview",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorReview_AppointmentId",
                schema: "doctors",
                table: "DoctorReview",
                column: "AppointmentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorReview_AppointmentId",
                schema: "doctors",
                table: "DoctorReview");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                schema: "doctors",
                table: "DoctorReview");

            migrationBuilder.DropColumn(
                name: "PatientFirstName",
                schema: "doctors",
                table: "DoctorReview");

            migrationBuilder.DropColumn(
                name: "PatientLastName",
                schema: "doctors",
                table: "DoctorReview");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "doctors",
                table: "DoctorReview");
        }
    }
}
