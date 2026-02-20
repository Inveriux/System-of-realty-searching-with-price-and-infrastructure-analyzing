namespace ListingService.Domain.Models;

public class ListingMedia
{
    public Guid Id { get; set; }
    //TODO Is need MediaType property?
    public string? MediaType { get; set; }
    public string? Url { get; set; }
    public string? PreviewUrl { get; set; }
    public int? SortOrder { get; set; }

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
}
