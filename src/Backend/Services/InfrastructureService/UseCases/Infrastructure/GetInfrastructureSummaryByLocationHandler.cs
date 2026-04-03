using BasicBuildingBlocks.BusinessResults;
using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Services.Summaries.Abstractions;
using MediatR;

namespace InfrastructureService.UseCases.Infrastructure;

public sealed record GetInfrastructureSummaryByLocationQuery(Guid LocationId) : IRequest<Result<InfrastructureSummaryDto>>;

public sealed class GetInfrastructureSummaryByLocationHandler(
    ISender sender,
    ILocationRepository locationRepository,
    IInfrastructureRepository infrastructureRepository,
    IInfrastructureSummaryBuilder summaryBuilder)
    : IRequestHandler<GetInfrastructureSummaryByLocationQuery, Result<InfrastructureSummaryDto>>
{
    public async Task<Result<InfrastructureSummaryDto>> Handle(GetInfrastructureSummaryByLocationQuery request, CancellationToken cancellationToken)
    {
        var location = await locationRepository.GetByIdAsync(request.LocationId, cancellationToken);
        if (location is null)
            return Result.NotFound<InfrastructureSummaryDto>("Локация не найдена.");

        var scoreResult = await sender.Send(new GetInfrastructureByLocationQuery(request.LocationId), cancellationToken);
        if (!scoreResult.IsSuccess || scoreResult.Data is null)
            return Result.Fail<InfrastructureSummaryDto>(scoreResult.StatusCode, scoreResult.Message, scoreResult.Errors);

        var poisResult = await sender.Send(new GetInfrastructurePoisByLocationQuery(request.LocationId), cancellationToken);
        if (!poisResult.IsSuccess || poisResult.Data is null)
            return Result.Fail<InfrastructureSummaryDto>(poisResult.StatusCode, poisResult.Message, poisResult.Errors);

        var snapshotItems = await infrastructureRepository.GetSnapshotItemsAsync(request.LocationId, cancellationToken);

        return Result.Ok(new InfrastructureSummaryDto
        {
            LocationId = location.Id,
            AddressText = location.AddressText,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Score = scoreResult.Data,
            Summary = summaryBuilder.BuildSummary(scoreResult.Data.ToModel(location.Id), scoreResult.Data.Details, snapshotItems),
            Highlights = scoreResult.Data.Details.Highlights.Count > 0
                ? scoreResult.Data.Details.Highlights
                : summaryBuilder.BuildHighlights(scoreResult.Data.ToModel(location.Id), scoreResult.Data.Details, snapshotItems),
            Pois = poisResult.Data,
            NearbyCategories = scoreResult.Data.Details.NearbyCategories,
            TransportDetails = scoreResult.Data.Details.TransportDetails,
            SearchFeatures = scoreResult.Data.Details.SearchFeatures,
            PrimaryRadiusConfig = scoreResult.Data.Details.PrimaryRadiusConfig,
            FallbackRadiusConfig = scoreResult.Data.Details.FallbackRadiusConfig,
            MissingPrimaryCategories = scoreResult.Data.Details.MissingPrimaryCategories,
            ClassificationVersions = scoreResult.Data.Details.ClassificationVersions
        });
    }
}
