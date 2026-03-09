using UserService.UseCases.Users;
using FluentValidation;

namespace UserService.Validations.Users;

public class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(x => x.SignInUserDto)
            .NotNull()
            .WithMessage("Sign in data must be provided.");

        When(x => x.SignInUserDto is not null, () =>
        {
            RuleFor(x => x.SignInUserDto.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Email format is invalid.");

            RuleFor(x => x.SignInUserDto.Password)
                .NotEmpty()
                .WithMessage("Password is required.");
        });
    }
}
