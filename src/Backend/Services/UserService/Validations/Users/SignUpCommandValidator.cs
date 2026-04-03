using UserService.Data.Repositiroes.Abstractions;
using Microsoft.AspNetCore.Identity;
using UserService.UseCases.Users;
using UserService.Models;
using FluentValidation;

namespace UserService.Validations.Users;

public class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
    public SignUpCommandValidator(IUserRepository userRepository, UserManager<User> userManager)
    {
        RuleFor(x => x.SignUpUserDto)
            .NotNull()
            .WithMessage("Необходимо передать данные для регистрации.");

        When(x => x.SignUpUserDto is not null, () =>
        {
            RuleFor(x => x.SignUpUserDto.Name)
                .NotEmpty()
                .WithMessage("Имя обязательно.");

            RuleFor(x => x.SignUpUserDto.Email)
                .NotEmpty()
                .WithMessage("Электронная почта обязательна.")
                .EmailAddress()
                .WithMessage("Некорректный формат email.")
                .MustAsync(async (email, cancellationToken) =>
                {
                    var user = await userRepository.GetByEmailAsync(email, false);
                    return user is null || !user.EmailConfirmed;
                })
                .WithMessage("Этот email уже используется подтвержденной учетной записью.");

            RuleFor(x => x.SignUpUserDto.Password)
                .NotEmpty()
                .WithMessage("Пароль обязателен.");

            RuleFor(x => x)
                .CustomAsync(async (command, context, cancellationToken) =>
                {
                    var dto = command.SignUpUserDto;
                    var user = User.Create(dto.Name, dto.Email, dto.AccountImageUrl);

                    var validationResults = await Task.WhenAll(userManager.PasswordValidators
                        .Select(validator => validator.ValidateAsync(userManager, user, dto.Password)));

                    foreach (var error in validationResults
                        .SelectMany(result => result.Errors)
                        .Select(error => error.Description)
                        .Distinct())
                    {
                        context.AddFailure(nameof(command.SignUpUserDto.Password), error);
                    }
                });
        });
    }
}
