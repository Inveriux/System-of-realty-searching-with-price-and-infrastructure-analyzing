using UserService.Data.Repositiroes.Abstractions;
using BasicBuildingBlocks.BusinessResults;
using Microsoft.AspNetCore.Identity;
using UserService.Models;
using UserService.DTOs;
using AutoMapper;
using MediatR;

namespace UserService.UseCases.Users;

public record GetUserByEmailQuery(string Email) : IRequest<Result<GetUserDto>>;

public class GetUserByEmailHandler(IUserRepository userRepository,
    UserManager<User> userManager,
    IMapper mapper) : IRequestHandler<GetUserByEmailQuery, Result<GetUserDto>>
{
    public async Task<Result<GetUserDto>> Handle(GetUserByEmailQuery request, 
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if(user is null)
            return Result.Fail<GetUserDto>("User not found.")
                .WithError($"User with {request.Email} email not found.", propertyName: nameof(User));

        var userDto = mapper.Map<GetUserDto>(user);
        var userRole = await userRepository.GetUserRoleAsync(user);
        userDto.Role = userRole 
            ?? throw new Exception("User role not found.");

        return Result.Ok(userDto);
    }
}
