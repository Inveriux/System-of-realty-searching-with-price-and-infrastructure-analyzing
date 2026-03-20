using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.DTOs.Infrastructure;
using MediatR;

namespace InfrastructureService.UseCases.Infrastructure;

public sealed record GetInfrastructurePoisByLocationQuery(Guid LocationId) : IRequest<Result<IReadOnlyCollection<PoiDto>>>;

public sealed class GetInfrastructurePoisByLocationHandler(IInfrastructureRepository infrastructureRepository, IMapper mapper)
    : IRequestHandler<GetInfrastructurePoisByLocationQuery, Result<IReadOnlyCollection<PoiDto>>>
{
    public async Task<Result<IReadOnlyCollection<PoiDto>>> Handle(GetInfrastructurePoisByLocationQuery request, CancellationToken cancellationToken)
    {
        var items = await infrastructureRepository.GetSnapshotItemsAsync(request.LocationId, cancellationToken);
        return Result.Ok(mapper.Map<IReadOnlyCollection<PoiDto>>(items));
    }
}
