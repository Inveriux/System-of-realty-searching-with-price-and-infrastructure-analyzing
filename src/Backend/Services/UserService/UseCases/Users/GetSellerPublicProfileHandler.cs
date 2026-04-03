using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using MediatR;
using UserService.Data.Repositiroes.Abstractions;
using UserService.DTOs;

namespace UserService.UseCases.Users;

public sealed record GetSellerPublicProfileQuery(Guid Id) : IRequest<Result<SellerPublicProfileDto>>;

public sealed class GetSellerPublicProfileHandler(
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetSellerPublicProfileQuery, Result<SellerPublicProfileDto>>
{
    public async Task<Result<SellerPublicProfileDto>> Handle(
        GetSellerPublicProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, false);
        if (user is null)
            return Result.NotFound<SellerPublicProfileDto>("Продавец не найден.");

        return Result.Ok(mapper.Map<SellerPublicProfileDto>(user));
    }
}
