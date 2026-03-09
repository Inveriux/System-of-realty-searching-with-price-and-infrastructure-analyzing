namespace ListingService.Domain.Abstractions;
public interface IEntity<TKey> 
{
    public TKey Id { get; set; }
}

public interface IAuditableEntity
{
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}