using FluentValidation;

namespace Modules.Identity.Core.DTOs;

public class ResetPasswordRequestValidation : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Token)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(6);
    }
}