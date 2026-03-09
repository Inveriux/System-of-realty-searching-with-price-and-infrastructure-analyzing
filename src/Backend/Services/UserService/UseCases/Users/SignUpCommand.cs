using UserService.Data.Repositiroes.Abstractions;
using BasicBuildingBlocks.Results;
using UserService.Models.Enums;
using UserService.Models;
using UserService.DTOs;
using AutoMapper;
using MediatR;

namespace UserService.UseCases.Users;

public record SignUpCommand(SignUpUserDto SignUpUserDto) : IRequest<Result<bool>>;

public class SignUpHandler(IUserRepository userRepository, 
    IMapper mapper) : IRequestHandler<SignUpCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(SignUpCommand request, CancellationToken cancellationToken = default)
    {
        var dto = request.SignUpUserDto;
        var user = mapper.Map<User>(dto);

        await userRepository.CreateUserAsync(user, dto.Password, UserRoles.User); //TODO Base role
        await userRepository.SaveChangesAsync();
        return Result.Ok(true);
    }
}
