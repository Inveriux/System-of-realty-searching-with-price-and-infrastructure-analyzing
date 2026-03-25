using BasicBuildingBlocks.BusinessResults;
using InfrastructureService.DTOs.Infrastructure;
using InfrastructureService.Services.Classification.Abstractions;
using InfrastructureService.Services.Configuration;
using MediatR;

namespace InfrastructureService.UseCases.Infrastructure;

public sealed record GetInfrastructureConfigQuery() : IRequest<Result<InfrastructureConfigDto>>;

public sealed class GetInfrastructureConfigHandler(IPoiClassificationService poiClassificationService)
    : IRequestHandler<GetInfrastructureConfigQuery, Result<InfrastructureConfigDto>>
{
    public Task<Result<InfrastructureConfigDto>> Handle(GetInfrastructureConfigQuery request, CancellationToken cancellationToken)
    {
        var dto = new InfrastructureConfigDto
        {
            Categories = InfrastructureMetadata.SupportedSubcategories
                .Select(static x => new InfrastructureSubcategoryConfigDto
                {
                    CategoryCode = x.CategoryCode,
                    SubcategoryCode = x.SubcategoryCode,
                    PrimaryRadiusMeters = x.PrimaryRadiusMeters,
                    FallbackRadiusMeters = x.FallbackRadiusMeters,
                    FallbackEnabled = x.FallbackRadiusMeters.HasValue
                })
                .ToArray(),
            ClassificationVersions = poiClassificationService.GetDictionaryVersions()
        };

        return Task.FromResult(Result.Ok(dto));
    }
}
