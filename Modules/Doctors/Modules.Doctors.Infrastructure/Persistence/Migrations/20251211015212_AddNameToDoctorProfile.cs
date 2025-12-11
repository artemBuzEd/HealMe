using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Doctors.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNameToDoctorProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "doctors",
                table: "DoctorProfile",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "doctors",
                table: "DoctorProfile",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "doctors",
                table: "DoctorProfile");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "doctors",
                table: "DoctorProfile");
        }
    }
}
