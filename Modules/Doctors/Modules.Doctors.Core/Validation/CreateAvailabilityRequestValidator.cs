using FluentValidation;
using Modules.Doctors.Core.DTOs;

namespace Modules.Doctors.Core.Validation;

public class CreateAvailabilityRequestValidator : AbstractValidator<CreateAvailabilityRequest>
{
    public CreateAvailabilityRequestValidator()
    {
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime);
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
    }
}