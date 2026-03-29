using FluentValidation;
using UserService.UseCases.Users;

namespace UserService.Validations.Users;

public sealed class ResendEmailVerificationCodeCommandValidator : AbstractValidator<ResendEmailVerificationCodeCommand>
{
    public ResendEmailVerificationCodeCommandValidator()
    {
        RuleFor(x => x.Dto.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
