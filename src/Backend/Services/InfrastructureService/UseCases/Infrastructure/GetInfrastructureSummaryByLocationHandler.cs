using BasicBuildingBlocks.BusinessResults;
using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.DTOs.Infrastructure;
using MediatR;

namespace InfrastructureService.UseCases.Infrastructure;

public sealed record GetInfrastructureSummaryByLocationQuery(Guid LocationId) : IRequest<Result<InfrastructureSummaryDto>>;

public sealed class GetInfrastructureSummaryByLocationHandler(ISender sender, ILocationRepository locationRepository)
    : IRequestHandler<GetInfrastructureSummaryByLocationQuery, Result<InfrastructureSummaryDto>>
{
    public async Task<Result<InfrastructureSummaryDto>> Handle(GetInfrastructureSummaryByLocationQuery request, CancellationToken cancellationToken)
    {
        var location = await locationRepository.GetByIdAsync(request.LocationId, cancellationToken);
        if (location is null)
            return Result.NotFound<InfrastructureSummaryDto>("Location was not found.");

        var scoreResult = await sender.Send(new GetInfrastructureByLocationQuery(request.LocationId), cancellationToken);
        if (!scoreResult.IsSuccess || scoreResult.Data is null)
            return Result.Fail<InfrastructureSummaryDto>(scoreResult.StatusCode, scoreResult.Message, scoreResult.Errors);

        var poisResult = await sender.Send(new GetInfrastructurePoisByLocationQuery(request.LocationId), cancellationToken);
        if (!poisResult.IsSuccess || poisResult.Data is null)
            return Result.Fail<InfrastructureSummaryDto>(poisResult.StatusCode, poisResult.Message, poisResult.Errors);

        return Result.Ok(new InfrastructureSummaryDto
        {
            LocationId = location.Id,
            AddressText = location.AddressText,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Score = scoreResult.Data,
            Pois = poisResult.Data
        });
    }
}
