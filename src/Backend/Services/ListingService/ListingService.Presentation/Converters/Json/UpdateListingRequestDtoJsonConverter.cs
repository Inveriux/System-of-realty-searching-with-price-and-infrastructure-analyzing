using System.Text.Json;
using System.Text.Json.Serialization;
using ListingService.Application.DTOs.Listings;
using ListingService.Application.DTOs.Listings.Abstractions;
using ListingService.Domain.Enums;
using ListingService.Presentation.Dtos.Listings;

namespace ListingService.Presentation.Converters.Json;

public sealed class UpdateListingRequestDtoJsonConverter : JsonConverter<UpdateListingRequestDto>
{
    public override UpdateListingRequestDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var listingJsonDto = JsonSerializer.Deserialize<CreateListingJsonDto>(root, options)
            ?? throw new JsonException("Failed to deserialize update listing payload.");

        if (!root.TryGetProperty("listingDetails", out var detailsJson))
            throw new JsonException("Missing 'listingDetails'.");

        var details = DeserializeDetails(listingJsonDto.ListingDetailsType, detailsJson, options);
        return listingJsonDto.ToUpdateApplicationDto(details);
    }

    public override void Write(Utf8JsonWriter writer, UpdateListingRequestDto value, JsonSerializerOptions options)
        => throw new NotImplementedException();

    private static IListingDetailsDto DeserializeDetails(string detailsType, JsonElement raw, JsonSerializerOptions options)
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
