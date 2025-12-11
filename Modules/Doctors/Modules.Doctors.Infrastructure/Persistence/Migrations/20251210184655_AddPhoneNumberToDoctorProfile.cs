using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Doctors.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneNumberToDoctorProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
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
                name: "PhoneNumber",
                schema: "doctors",
                table: "DoctorProfile");
        }
    }
}
