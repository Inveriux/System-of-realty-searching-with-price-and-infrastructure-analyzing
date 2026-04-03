using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.DTOs.Locations;
using MediatR;

namespace InfrastructureService.UseCases.Locations;

public sealed record GetLocationByCoordinatesQuery(decimal Latitude, decimal Longitude) : IRequest<Result<LocationDto>>;

public sealed class GetLocationByCoordinatesHandler(ILocationRepository locationRepository, IMapper mapper)
    : IRequestHandler<GetLocationByCoordinatesQuery, Result<LocationDto>>
{
    public async Task<Result<LocationDto>> Handle(GetLocationByCoordinatesQuery request, CancellationToken cancellationToken)
    {
        var location = await locationRepository.GetByCoordinatesAsync(request.Latitude, request.Longitude, cancellationToken);
        return location is null
            ? Result.NotFound<LocationDto>("Локация не найдена.")
            : Result.Ok(mapper.Map<LocationDto>(location));
    }
}
