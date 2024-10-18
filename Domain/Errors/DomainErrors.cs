using Domain.Shared;

namespace Domain.Errors
{
    public static class DomainErrors
    {
        public static class ValueObject
        {
            public static class Email
            {
                public static readonly Error NullOrWhiteSpace = new("email IsNullOrWhiteSpace", "email  is Null Or White Space.");
                public static readonly Error MaxLengthExceeded = new("email MaxLength", "email is longer than allowed max length");
                public static readonly Error InvalidFormat = new("email InvalidFormat", "The email format is invalid.");

            }
        }
    }
}