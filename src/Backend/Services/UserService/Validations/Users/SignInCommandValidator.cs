using UserService.UseCases.Users;
using FluentValidation;

namespace UserService.Validations.Users;

public class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(x => x.SignInUserDto)
            .NotNull()
            .WithMessage("Необходимо передать данные для входа.");

        When(x => x.SignInUserDto is not null, () =>
        {
            RuleFor(x => x.SignInUserDto.Email)
                .NotEmpty()
                .WithMessage("Электронная почта обязательна.")
                .EmailAddress()
                .WithMessage("Некорректный формат email.");

            RuleFor(x => x.SignInUserDto.Password)
                .NotEmpty()
                .WithMessage("Пароль обязателен.");
        });
    }
}
