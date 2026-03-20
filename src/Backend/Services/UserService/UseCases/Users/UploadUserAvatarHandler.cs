using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using MediatR;
using UserService.Data.Repositiroes.Abstractions;
using UserService.DTOs;
using UserService.Models;

namespace UserService.UseCases.Users;

public sealed record UploadUserAvatarCommand(string Email, string AvatarUrl) : IRequest<Result<GetUserDto>>;

public sealed class UploadUserAvatarCommandHandler(IUserRepository userRepository, IMapper mapper)
    : IRequestHandler<UploadUserAvatarCommand, Result<GetUserDto>>
{
    public async Task<Result<GetUserDto>> Handle(UploadUserAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, true);
        if (user is null)
        {
            return Result.Fail<GetUserDto>("User not found.")
                .WithError($"User with {request.Email} email not found.", propertyName: nameof(User));
        }

        user.AccountImageUrl = request.AvatarUrl;
        await userRepository.SaveChangesAsync();

        var userDto = mapper.Map<GetUserDto>(user);
        userDto.Role = await userRepository.GetUserRoleAsync(user) ?? string.Empty;

        return Result.Ok(userDto, "Avatar updated.");
    }
}
