namespace ListingService.Domain.Models;

public class ListingMedia
{
    public Guid Id { get; set; }
    public bool IsMainMedia { get; set; } = false;
    public string Url { get; set; } = null!;

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
}
