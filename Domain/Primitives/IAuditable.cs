namespace Domain.Primitives;

public interface IAuditable
{
    public DateTime? LastUpdateDateUtc { get; set; }
    public Guid? LastUpdatedBy { get; set; }

    public string ItemType { get; }
}