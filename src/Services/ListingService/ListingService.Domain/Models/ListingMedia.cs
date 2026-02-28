namespace ListingService.Domain.Models;

public class ListingMedia
{
    public Guid Id { get; set; }
    //TODO Is need MediaType property?
    public bool IsMainMedia { get; set; } = false;
    public string? Url { get; set; }

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
}
