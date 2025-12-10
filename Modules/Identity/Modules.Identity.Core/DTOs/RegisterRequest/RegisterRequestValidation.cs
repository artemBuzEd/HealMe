using System.Data;
using FluentValidation;

namespace Modules.Identity.Core.DTOs;

public class RegisterRequestValidation : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email is required.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.FirstName)
            .NotEmpty();
        
        RuleFor(x => x.LastName)
            .NotEmpty();
    }
}