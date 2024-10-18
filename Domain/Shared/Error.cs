namespace Domain.Shared
{
    public class Error : IEquatable<Error>
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");

        public Error(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public string Code { get; }
        public string Message { get; }

        //when cast to string
        public static implicit operator string(Error error)
        {
            return error.Code;
        }

        public static bool operator ==(Error? a, Error? b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            return a is not null && b is not null && a.Code == b.Code;
        }

        public static bool operator !=(Error? a, Error? b)
        {
            return !(a == b);
        }

        public bool Equals(Error? other)
        {
            return this == other;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }
            return obj is Error && Equals((Error)obj);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }
    }
}