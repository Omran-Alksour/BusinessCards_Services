using Domain.Errors;
using Domain.Primitives;
using Domain.Shared;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    public sealed class Email : ValueObject
    {
        private Email(string value)
        {
            Value = value;
        }

        public const int MaxLength = 128;
        public string Value { get; }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        public static Result<Email> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result.Failure<Email>(DomainErrors.ValueObject.Email.NullOrWhiteSpace);
            }
            if ( value.Length > MaxLength)
            {
                return Result.Failure<Email>(DomainErrors.ValueObject.Email.MaxLengthExceeded);
            }
            if (!IsValid(value))
            {
                return Result.Failure<Email>(DomainErrors.ValueObject.Email.InvalidFormat);
            }

            return Result.Success(new Email(value));
        }

        public static bool IsValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            Regex regex = new Regex("^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$", RegexOptions.IgnoreCase);
            return regex.IsMatch(email) && email.Length <=MaxLength;
        }
    }
}
