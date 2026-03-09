using ListingService.Application.DTOs.Listings.Abstractions;
using ListingService.Presentation.Dtos.Listings;
using ListingService.Application.DTOs.Listings;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ListingService.Presentation.Converters.Json;

public sealed class CreateListingDtoJsonConverter : JsonConverter<CreateListingDto>
{
    public override CreateListingDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var listingJsonDto = JsonSerializer.Deserialize<CreateListingJsonDto>(root, options)
            ?? throw new JsonException("Failed to deserialize CreateListingJsonDto.");

        if (!root.TryGetProperty("listingDetails", out var detailsJson))
            throw new JsonException("Missing 'listingDetails' inside 'listingDto'.");

        var details = DeserializeDetails(listingJsonDto.ListingDetailsType, detailsJson, options);

        return listingJsonDto.ToApplicationDto(details);
    }

    public override void Write(Utf8JsonWriter writer, CreateListingDto value, JsonSerializerOptions options)
        => throw new NotImplementedException();

    private static IListingDetailsDto DeserializeDetails(
        string detailsType,
        JsonElement raw,
        JsonSerializerOptions options)
    {
        if (raw.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            throw new JsonException("listingDetails is missing or null.");

        var json = raw.GetRawText();

        return detailsType switch
        {
            ListingDetailsType.Apartment =>
                JsonSerializer.Deserialize<ApartmentDetailsDto>(json, options)
                ?? throw new JsonException("Invalid Apartment listingDetails."),

            ListingDetailsType.House =>
                JsonSerializer.Deserialize<HouseDetailsDto>(json, options)
                ?? throw new JsonException("Invalid House listingDetails."),

            ListingDetailsType.Land =>
                JsonSerializer.Deserialize<LandDetailsDto>(json, options)
                ?? throw new JsonException("Invalid Land listingDetails."),

            ListingDetailsType.Warehouse =>
                JsonSerializer.Deserialize<WarehouseDetailsDto>(json, options)
                ?? throw new JsonException("Invalid Warehouse listingDetails."),

            ListingDetailsType.Office =>
                JsonSerializer.Deserialize<OfficeDetailsDto>(json, options)
                ?? throw new JsonException("Invalid Office listingDetails."),

            _ => throw new JsonException($"Unknown listingDetailsType: '{detailsType}'.")
        };
    }
}