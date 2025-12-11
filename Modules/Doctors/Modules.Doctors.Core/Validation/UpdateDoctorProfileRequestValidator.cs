using FluentValidation;
using Modules.Doctors.Core.DTOs;

namespace Modules.Doctors.Core.Validation;

public class UpdateDoctorProfileRequestValidator : AbstractValidator<UpdateDoctorProfileRequest>
{
    public UpdateDoctorProfileRequestValidator()
    {
        RuleFor(x => x.ConsultationFee).GreaterThan(0).WithMessage("Doctor consultation fee must be greater than 0");
        RuleFor(x => x.MedicalInstitutionLicense).NotNull();
        RuleFor(x => x.PhoneNumber).NotEmpty().NotNull().WithMessage("Phone number is required.");
    }
}