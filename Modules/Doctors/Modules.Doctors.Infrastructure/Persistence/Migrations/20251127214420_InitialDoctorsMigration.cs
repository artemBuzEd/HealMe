using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Doctors.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialDoctorsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "doctors");

            migrationBuilder.CreateTable(
                name: "DoctorProfile",
                schema: "doctors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SpecializationId = table.Column<string>(type: "text", nullable: false),
                    ConsultationFee = table.Column<double>(type: "double precision", nullable: false),
                    MedicalInstitutionLicense = table.Column<string>(type: "text", nullable: false),
                    Biography = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorProfile", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorProfile",
                schema: "doctors");
        }
    }
}
