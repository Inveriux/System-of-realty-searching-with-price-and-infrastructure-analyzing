using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.DTOs.Locations;
using MediatR;

namespace InfrastructureService.UseCases.Locations;

public sealed record GetLocationByIdQuery(Guid Id) : IRequest<Result<LocationDto>>;

public sealed class GetLocationByIdHandler(ILocationRepository locationRepository, IMapper mapper)
    : IRequestHandler<GetLocationByIdQuery, Result<LocationDto>>
{
    public async Task<Result<LocationDto>> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
    {
        var location = await locationRepository.GetByIdAsync(request.Id, cancellationToken);
        return location is null
            ? Result.NotFound<LocationDto>("Location was not found.")
            : Result.Ok(mapper.Map<LocationDto>(location));
    }
}
