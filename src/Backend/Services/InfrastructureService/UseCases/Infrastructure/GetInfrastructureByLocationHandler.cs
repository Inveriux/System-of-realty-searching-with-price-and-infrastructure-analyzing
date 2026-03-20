using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.DTOs.Infrastructure;
using MediatR;

namespace InfrastructureService.UseCases.Infrastructure;

public sealed record GetInfrastructureByLocationQuery(Guid LocationId) : IRequest<Result<InfrastructureScoreDto>>;

public sealed class GetInfrastructureByLocationHandler(IInfrastructureRepository infrastructureRepository, IMapper mapper)
    : IRequestHandler<GetInfrastructureByLocationQuery, Result<InfrastructureScoreDto>>
{
    public async Task<Result<InfrastructureScoreDto>> Handle(GetInfrastructureByLocationQuery request, CancellationToken cancellationToken)
    {
        var score = await infrastructureRepository.GetScoreAsync(request.LocationId, cancellationToken);
        return score is null
            ? Result.NotFound<InfrastructureScoreDto>("Infrastructure score was not found.")
            : Result.Ok(mapper.Map<InfrastructureScoreDto>(score));
    }
}
