namespace Domain.Primitives
{
    public abstract class EntityT<TValue> : IEquatable<EntityT<TValue>>
    {
        protected EntityT(TValue id)
        {
            Id = id;
        }

        public TValue Id { get; private init; }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return obj is EntityT<TValue> && Id.Equals(((EntityT<TValue>)obj).Id);
        }

        public bool Equals(EntityT<TValue>? other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.GetType() != GetType())
            {
                return false;
            }

            return other is not null && Id.Equals(other.Id);
        }

        public override int GetHashCode()
        { return Id.GetHashCode(); }

        public static bool operator ==(EntityT<TValue>? first, EntityT<TValue>? second)
        {
            if (first is null)
            {
                return false;
            }

            return second is not null && first.Equals(second);
        }

        public static bool operator !=(EntityT<TValue>? first, EntityT<TValue>? second)
        {
            return !(first == second);
        }

        public bool IsDeleted
        {
            get; private set;
        }

        public virtual void Delete()
        {
            if (this is IDeletable) //only work if IDeletable
            {
                IsDeleted = true;
            }
        }

        public virtual void UnDelete()
        {
            if (this is IDeletable) //only work if IDeletable
            {
                IsDeleted = false;
            }
        }
    }
}