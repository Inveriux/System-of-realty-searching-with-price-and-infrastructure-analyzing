using UserService.Data.Repositiroes.Abstractions;
using BasicBuildingBlocks.BusinessResults;
using Microsoft.AspNetCore.Identity;
using UserService.Models;
using UserService.DTOs;
using MediatR;

namespace UserService.UseCases.Users;

public record ChangePasswordCommand(ChangePasswordDto ChangePasswordDto) 
    : IRequest<Result<bool>>;

public class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    UserManager<User> userManager) 
    : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var dto = request.ChangePasswordDto;
        var user = await userRepository.GetByEmailAsync(dto.Email, true);
        if (user is null)
            return Result.Fail<bool>("User not found.")
                .WithError($"User with {dto.Email} email not found.", propertyName: nameof(User));

        IdentityResult result;
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            result = await userManager.AddPasswordAsync(user, dto.NewPassword);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            {
                return Result.Fail<bool>("Current password is required.")
                    .WithError("Current password is required.", propertyName: nameof(ChangePasswordDto.CurrentPassword));
            }

            result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        }

        if (!result.Succeeded)
        {
            var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result.Fail<bool>("Failed to change password.")
                .WithError(errorMessage, propertyName: nameof(ChangePasswordDto.NewPassword));
        }

        return Result.Ok(true);
    }
}
