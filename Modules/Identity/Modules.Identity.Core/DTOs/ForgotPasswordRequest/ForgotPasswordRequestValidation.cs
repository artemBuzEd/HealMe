using FluentValidation;

namespace Modules.Identity.Core.DTOs;

public class ForgotPasswordRequestValidation : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email is required.");
        
    }
}