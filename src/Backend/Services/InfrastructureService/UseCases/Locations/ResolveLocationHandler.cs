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
                return Result.NotFound<LocationDto>("Не удалось определить локацию.");

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
            return Result.Validation<LocationDto>("Необходимо передать `addressText` или координаты.");

        var latitude = request.Request.Latitude.Value;
        var longitude = request.Request.Longitude.Value;

        var byCoordinates = await locationRepository.GetByCoordinatesAsync(latitude, longitude, cancellationToken);
        if (byCoordinates is not null)
            return Result.Ok(mapper.Map<LocationDto>(byCoordinates));

        var reverseResolved = await yandexGeocoderClient.ResolveAsync(latitude, longitude, cancellationToken);
        if (reverseResolved is null)
            return Result.NotFound<LocationDto>("Не удалось определить адрес по точке на карте.");

        var reverseResolvedCached = await locationRepository.GetByNormalizedAddressAsync(reverseResolved.NormalizedAddressText, cancellationToken);
        if (reverseResolvedCached is not null)
            return Result.Ok(mapper.Map<LocationDto>(reverseResolvedCached));

        var reverseResolvedLocation = new Location
        {
            Id = Guid.NewGuid(),
            AddressText = reverseResolved.AddressText,
            NormalizedAddressText = reverseResolved.NormalizedAddressText,
            Latitude = reverseResolved.Latitude,
            Longitude = reverseResolved.Longitude,
            Country = reverseResolved.Country,
            Region = reverseResolved.Region,
            City = reverseResolved.City,
            Street = reverseResolved.Street,
            HouseNumber = reverseResolved.HouseNumber,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await locationRepository.AddAsync(reverseResolvedLocation, cancellationToken);
        try
        {
            await locationRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var concurrentLocation = await locationRepository.GetByNormalizedAddressAsync(reverseResolved.NormalizedAddressText, cancellationToken);
            if (concurrentLocation is not null)
                return Result.Ok(mapper.Map<LocationDto>(concurrentLocation));

            throw;
        }

        return Result.Ok(mapper.Map<LocationDto>(reverseResolvedLocation));
    }
}
