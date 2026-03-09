using UserService.Data.Repositiroes.Abstractions;
using BasicBuildingBlocks.Results;
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
            return Result.Fail<GetUserDto>("User not found.")
                .WithError($"User with {request.Id} id not found.");

        var userDto = mapper.Map<GetUserDto>(user);
        return Result.Ok(userDto);
    }
}