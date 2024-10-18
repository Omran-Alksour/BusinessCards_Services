using Domain.Shared;

namespace Domain.Errors
{
    public static class PersistenceErrors
    {
        public static class BaseRepository
        {
            public static class UnDelete
            {
                public static readonly Error EntityNotIDeletable = new("Entity Not IDeletable", "Entity is not of IDeletable Type");
            }
        }
    }
}