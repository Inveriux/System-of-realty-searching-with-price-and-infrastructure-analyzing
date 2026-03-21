using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using BasicBuildingBlocks.BusinessResults.Extensoins;
using BasicBuildingBlocks.Services.Abstractions;
using MediatR;
using UserService.Data.Repositiroes.Abstractions;
using UserService.DTOs;
using UserService.Models;

namespace UserService.UseCases.Users;

public sealed record UpdateOwnProfileCommand(UpdateOwnProfileDto Dto) : IRequest<Result<GetUserDto>>;

public sealed class UpdateOwnProfileCommandHandler(
    IUserRepository userRepository,
    ICurrentUserProvider currentUserProvider,
    IMapper mapper)
    : IRequestHandler<UpdateOwnProfileCommand, Result<GetUserDto>>
{
    public async Task<Result<GetUserDto>> Handle(UpdateOwnProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserProvider.GetUserId();
        var user = await userRepository.GetByIdAsync(userId, true);

        if (user is null)
        {
            return Result.Fail<GetUserDto>("User not found.")
                .WithError($"User with {userId} id not found.", propertyName: nameof(User));
        }

        user.Name = request.Dto.Name.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.Dto.PhoneNumber)
            ? null
            : request.Dto.PhoneNumber.Trim();

        await userRepository.SaveChangesAsync();

        var userDto = mapper.Map<GetUserDto>(user);
        userDto.Role = await userRepository.GetUserRoleAsync(user) ?? string.Empty;

        return Result.Ok(userDto, "Profile updated.");
    }
}
