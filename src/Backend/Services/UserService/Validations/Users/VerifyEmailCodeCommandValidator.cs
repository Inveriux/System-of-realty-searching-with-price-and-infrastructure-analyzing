using FluentValidation;
using UserService.UseCases.Users;

namespace UserService.Validations.Users;

public sealed class VerifyEmailCodeCommandValidator : AbstractValidator<VerifyEmailCodeCommand>
{
    public VerifyEmailCodeCommandValidator()
    {
        RuleFor(x => x.Dto.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Dto.Code)
            .NotEmpty()
            .Length(6)
            .Matches(@"^\d{6}$")
            .WithMessage("Verification code must contain 6 digits.");
    }
}
