using Microsoft.AspNetCore.Identity;
using UserService.Data.Repositiroes.Abstractions;
using UserService.UseCases.Users;
using UserService.Models;
using FluentValidation;

namespace UserService.Validations.Users;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator(
        UserManager<User> userManager,
        IUserRepository userRepository)
    {
        RuleFor(x => x.ChangePasswordDto)
            .NotNull()
            .WithMessage("Необходимо передать данные для смены пароля.");

        When(x => x.ChangePasswordDto is not null, () =>
        {
            RuleFor(x => x.ChangePasswordDto.Email)
                .NotEmpty()
                .WithMessage("Электронная почта обязательна.")
                .EmailAddress()
                .WithMessage("Некорректный формат email.");

            RuleFor(x => x.ChangePasswordDto.NewPassword)
                .NotEmpty()
                .WithMessage("Новый пароль обязателен.")
                .NotEqual(x => x.ChangePasswordDto.CurrentPassword!)
                .WithMessage("Новый пароль должен отличаться от текущего.");

            RuleFor(x => x)
                .CustomAsync(async (command, context, cancellationToken) =>
                {
                    var dto = command.ChangePasswordDto;
                    var user = await userRepository.GetByEmailAsync(dto.Email, false);
                    if (user is null)
                        return;

                    if (!string.IsNullOrWhiteSpace(user.PasswordHash) && string.IsNullOrWhiteSpace(dto.CurrentPassword))
                    {
                        context.AddFailure(nameof(command.ChangePasswordDto.CurrentPassword), "Текущий пароль обязателен.");
                    }
                });

            RuleFor(x => x)
                .CustomAsync(async (command, context, cancellationToken) =>
                {
                    var dto = command.ChangePasswordDto;
                    var user = User.Create("temp-user", dto.Email);

                    var validationResults = await Task.WhenAll(userManager.PasswordValidators
                        .Select(validator => validator.ValidateAsync(userManager, user, dto.NewPassword)));

                    foreach (var error in validationResults
                        .SelectMany(result => result.Errors)
                        .Select(error => error.Description)
                        .Distinct())
                    {
                        context.AddFailure(nameof(command.ChangePasswordDto.NewPassword), error);
                    }
                });
        });
    }
}
