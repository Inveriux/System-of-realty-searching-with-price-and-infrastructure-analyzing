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
            .WithMessage("Password change data must be provided.");

        When(x => x.ChangePasswordDto is not null, () =>
        {
            RuleFor(x => x.ChangePasswordDto.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Email format is invalid.");

            RuleFor(x => x.ChangePasswordDto.NewPassword)
                .NotEmpty()
                .WithMessage("New password is required.")
                .NotEqual(x => x.ChangePasswordDto.CurrentPassword!)
                .WithMessage("New password must differ from current password.");

            RuleFor(x => x)
                .CustomAsync(async (command, context, cancellationToken) =>
                {
                    var dto = command.ChangePasswordDto;
                    var user = await userRepository.GetByEmailAsync(dto.Email, false);
                    if (user is null)
                        return;

                    if (!string.IsNullOrWhiteSpace(user.PasswordHash) && string.IsNullOrWhiteSpace(dto.CurrentPassword))
                    {
                        context.AddFailure(nameof(command.ChangePasswordDto.CurrentPassword), "Current password is required.");
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
