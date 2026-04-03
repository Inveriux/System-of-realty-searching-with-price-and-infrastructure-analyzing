using UserService.Data.Repositiroes.Abstractions;
using BasicBuildingBlocks.Services.Abstractions;
using BasicBuildingBlocks.BusinessResults;
using UserService.Models;
using UserService.DTOs;
using AutoMapper;
using MediatR;

namespace UserService.UseCases.Users;

public record GetUserOwnInfoQuery : IRequest<Result<GetUserDto>>;

public class GetUserOwnInfoHandler(IUserRepository userRepository, 
    ICurrentUserProvider currentUserProvider,
    IMapper mapper) : IRequestHandler<GetUserOwnInfoQuery, Result<GetUserDto>>
{
    public async Task<Result<GetUserDto>> Handle(GetUserOwnInfoQuery request, 
        CancellationToken cancellationToken = default)
    {
        var userId = currentUserProvider.GetUserId();

        var user = await userRepository.GetByIdAsync(userId, false);
        
        if (user is null)
            return Result.Fail<GetUserDto>("Пользователь не найден.")
                .WithError("Пользователь с таким идентификатором не найден.", propertyName: nameof(User));

        var userDto = mapper.Map<GetUserDto>(user);
        var userRole = await userRepository.GetUserRoleAsync(user);
        userDto.Role = userRole
            ?? throw new Exception("User role not found.");

        return Result.Ok(userDto);
    }
}
