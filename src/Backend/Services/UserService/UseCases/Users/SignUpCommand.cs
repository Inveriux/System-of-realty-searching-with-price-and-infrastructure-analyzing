using UserService.Data.Repositiroes.Abstractions;
using BasicBuildingBlocks.BusinessResults;
using UserService.Services.EmailVerification.Abstractions;
using Microsoft.AspNetCore.Identity;
using UserService.Models.Enums;
using UserService.Models;
using UserService.DTOs;
using AutoMapper;
using MediatR;

namespace UserService.UseCases.Users;

public record SignUpCommand(SignUpUserDto SignUpUserDto) : IRequest<Result<bool>>;

public class SignUpHandler(
    IUserRepository userRepository,
    IMapper mapper,
    UserManager<User> userManager,
    IEmailVerificationService emailVerificationService) : IRequestHandler<SignUpCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(SignUpCommand request, CancellationToken cancellationToken = default)
    {
        var dto = request.SignUpUserDto;
        if (!emailVerificationService.IsConfigured)
            return Result.Fail<bool>("Подтверждение email не настроено.")
                .WithError("SMTP-отправитель не настроен.", propertyName: nameof(SignUpUserDto.Email));

        var existingUser = await userRepository.GetByEmailAsync(dto.Email, true);
        if (existingUser is not null)
        {
            if (existingUser.EmailConfirmed)
                return Result.Fail<bool>("Электронная почта уже используется.")
                    .WithError("Пользователь с таким email уже существует.", propertyName: nameof(User.Email));

            var deleteResult = await userManager.DeleteAsync(existingUser);
            if (!deleteResult.Succeeded)
                throw new Exception(string.Join("; ", deleteResult.Errors.Select(e => e.Description)));
        }

        var user = mapper.Map<User>(dto);

        await userRepository.CreateUserAsync(user, dto.Password, UserRoles.User); //TODO Base role
        await emailVerificationService.SendCodeAsync(user, cancellationToken);
        return Result.Ok(true);
    }
}
