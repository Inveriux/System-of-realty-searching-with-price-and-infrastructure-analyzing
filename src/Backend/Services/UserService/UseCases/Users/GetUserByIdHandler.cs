using UserService.Data.Repositiroes.Abstractions;
using BasicBuildingBlocks.BusinessResults;
using UserService.Models;
using UserService.DTOs;
using AutoMapper;
using MediatR;

namespace UserService.UseCases.Users;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<GetUserDto>>;

public class GetUserByIdHandler(IUserRepository userRepository, 
    IMapper mapper) 
    : IRequestHandler<GetUserByIdQuery, Result<GetUserDto>>
{
    public async Task<Result<GetUserDto>> Handle(GetUserByIdQuery request, 
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(request.Id, false);
        
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
