namespace ListingService.Domain.Abstractions;

public class AuditableEntity<TKey> : IEntity<TKey>, IAuditableEntity
{
    public required TKey Id { get; set; }
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}