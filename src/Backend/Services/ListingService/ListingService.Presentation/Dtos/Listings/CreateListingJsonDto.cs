using ListingService.Application.DTOs.Listings.Abstractions;
using ListingService.Application.DTOs.Listings;

namespace ListingService.Presentation.Dtos.Listings;

public sealed class CreateListingJsonDto
{
    public Guid AgentId { get; set; }
    public required string AddressText { get; init; } 
    public Guid? LocationId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public required string Title { get; init; }
    public string? Description { get; set; }
    public decimal? PriceAmount { get; set; }
    public string? PriceCurrency { get; set; }
    public decimal? PriceAmountByn { get; set; }
    public decimal? PriceAmountUsd { get; set; }
    public string? PricePeriod { get; set; }
    public decimal AreaTotal { get; set; }
    public int? BuildYear { get; set; }
    public DateTimeOffset? PublishedDate { get; set; }
    public Guid AddressId { get; set; }
    public required string BuildingType { get; set; }
    public required string ListingType { get; set; }
    public string? Status { get; set; }
    public required string ListingDetailsType { get; init; }


    public CreateListingDto ToApplicationDto(IListingDetailsDto listingDetails)
        => new()
        {
            AgentId = AgentId,
            AddressText = AddressText,
            LocationId = LocationId,
            Latitude = Latitude,
            Longitude = Longitude,
            Title = Title,
            Description = Description,
            PriceAmount = PriceAmount,
            PriceCurrency = PriceCurrency,
            PriceAmountByn = PriceAmountByn,
            PriceAmountUsd = PriceAmountUsd,
            PricePeriod = PricePeriod,
            AreaTotal = AreaTotal,
            BuildYear = BuildYear,
            PublishedDate = PublishedDate,
            AddressId = AddressId,
            BuildingType = BuildingType,
            ListingType = ListingType,
            Status = Status,
            ListingDetailsType = ListingDetailsType,
            ListingDetails = listingDetails
        };
}
