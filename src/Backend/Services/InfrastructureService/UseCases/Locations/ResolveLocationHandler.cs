using AutoMapper;
using BasicBuildingBlocks.BusinessResults;
using InfrastructureService.Data.Repositories.Abstractions;
using InfrastructureService.DTOs.Locations;
using InfrastructureService.Models;
using InfrastructureService.Services.Yandex.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureService.UseCases.Locations;

public sealed record ResolveLocationCommand(ResolveLocationRequestDto Request) : IRequest<Result<LocationDto>>;

public sealed class ResolveLocationHandler(
    ILocationRepository locationRepository,
    IYandexGeocoderClient yandexGeocoderClient,
    IMapper mapper) : IRequestHandler<ResolveLocationCommand, Result<LocationDto>>
{
    public async Task<Result<LocationDto>> Handle(ResolveLocationCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Request.AddressText))
        {
            var normalized = request.Request.AddressText.Trim().ToLowerInvariant();
            var cached = await locationRepository.GetByNormalizedAddressAsync(normalized, cancellationToken);
            if (cached is not null)
                return Result.Ok(mapper.Map<LocationDto>(cached));

            var resolved = await yandexGeocoderClient.ResolveAsync(request.Request.AddressText, cancellationToken);
            if (resolved is null)
                return Result.NotFound<LocationDto>("Location was not resolved.");

            var resolvedCached = await locationRepository.GetByNormalizedAddressAsync(resolved.NormalizedAddressText, cancellationToken);
            if (resolvedCached is not null)
                return Result.Ok(mapper.Map<LocationDto>(resolvedCached));

            var location = new Location
            {
                Id = Guid.NewGuid(),
                AddressText = resolved.AddressText,
                NormalizedAddressText = resolved.NormalizedAddressText,
                Latitude = resolved.Latitude,
                Longitude = resolved.Longitude,
                Country = resolved.Country,
                Region = resolved.Region,
                City = resolved.City,
                Street = resolved.Street,
                HouseNumber = resolved.HouseNumber,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await locationRepository.AddAsync(location, cancellationToken);
            try
            {
                await locationRepository.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                var concurrentLocation = await locationRepository.GetByNormalizedAddressAsync(resolved.NormalizedAddressText, cancellationToken);
                if (concurrentLocation is not null)
                    return Result.Ok(mapper.Map<LocationDto>(concurrentLocation));

                throw;
            }

            return Result.Ok(mapper.Map<LocationDto>(location));
        }

        if (request.Request.Latitude is null || request.Request.Longitude is null)
            return Result.Validation<LocationDto>("AddressText or coordinates are required.");

        var byCoordinates = await locationRepository.GetByCoordinatesAsync(
            request.Request.Latitude.Value,
            request.Request.Longitude.Value,
            cancellationToken);

        if (byCoordinates is null)
            return Result.NotFound<LocationDto>("Location by coordinates was not found.");

        return Result.Ok(mapper.Map<LocationDto>(byCoordinates));
    }
}
