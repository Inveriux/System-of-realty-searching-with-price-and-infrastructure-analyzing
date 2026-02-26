using System.Text.Json;
using System.Text.Json.Serialization;
using ListingService.Application.DTOs.Listings.Abstractions;

public sealed class ListingDetailsDtoWriteConverter : JsonConverter<IListingDetailsDto>
{
    public override IListingDetailsDto? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Reading IListingDetailsDto is not supported by this converter.");

    public override void Write(Utf8JsonWriter writer, IListingDetailsDto value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }


        System.Console.WriteLine(value.GetType());
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}