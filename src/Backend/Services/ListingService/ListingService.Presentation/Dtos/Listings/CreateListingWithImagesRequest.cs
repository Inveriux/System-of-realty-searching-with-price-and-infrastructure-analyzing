using Microsoft.AspNetCore.Mvc;

namespace ListingService.Presentation.Dtos.Listings;

public sealed class CreateListingWithImagesRequest
{
    [FromForm(Name = "listing")]
    public string Listing { get; set; } = string.Empty;

    [FromForm(Name = "images")]
    public List<IFormFile>? Images { get; set; } = [];
}
