namespace ListingService.Domain.Abstractions;
public interface IEntity<TKey> : IEntity
{
    public TKey Id { get; set; }
}

public interface IEntity
{
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}