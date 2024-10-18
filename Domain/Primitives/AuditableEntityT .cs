using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Primitives;

public abstract class AuditableEntityT<TValue> : EntityT<TValue>, IAuditable where TValue : IEquatable<TValue>
{
    protected AuditableEntityT(TValue id) : base(id)
    {
    }

    public DateTime? LastUpdateDateUtc { get; set; }

    public Guid? LastUpdatedBy { get; set; }
    public string ItemType => GetType().Name;
}