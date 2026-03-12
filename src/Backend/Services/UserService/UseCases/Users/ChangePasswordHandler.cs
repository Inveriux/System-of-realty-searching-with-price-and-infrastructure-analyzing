using UserService.Data.Repositiroes.Abstractions;
using BasicBuildingBlocks.BusinessResults;
using UserService.Models;
using UserService.DTOs;
using MediatR;

namespace UserService.UseCases.Users;

public record ChangePasswordCommand(ChangePasswordDto ChangePasswordDto) 
    : IRequest<Result<bool>>;

public class ChangePasswordCommandHandler(IUserRepository userRepository) 
    : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var dto = request.ChangePasswordDto;
        var user = await userRepository.GetByEmailAsync(dto.Email, false);
        if (user is null)
            return Result.Fail<bool>("User not found.")
                .WithError($"User with {dto.Email} email not found.", propertyName: nameof(User));

        return Result.Ok(true);
    }
}
