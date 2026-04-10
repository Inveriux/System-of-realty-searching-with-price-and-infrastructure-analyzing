using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using MediatR;
using UserService.Data.Repositiroes.Abstractions;
using UserService.DTOs;
using UserService.Models.Enums;

namespace UserService.UseCases.Users;

public sealed record GetSupportPublicProfileQuery() : IRequest<Result<SellerPublicProfileDto>>;

public sealed class GetSupportPublicProfileHandler(
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetSupportPublicProfileQuery, Result<SellerPublicProfileDto>>
{
    public async Task<Result<SellerPublicProfileDto>> Handle(GetSupportPublicProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetFirstByRoleAsync(UserRoles.Admin);
        if (user is null)
        {
            return Result.NotFound<SellerPublicProfileDto>("Оператор поддержки не найден.");
        }

        return Result.Ok(mapper.Map<SellerPublicProfileDto>(user));
    }
}
