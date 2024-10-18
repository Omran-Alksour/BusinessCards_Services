using Microsoft.AspNetCore.Http;

namespace Domain.Shared
{
    public class Result
    {
        protected internal Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
            {
                throw new InvalidOperationException();
            }
            if (!isSuccess && error == Error.None)
            {
                throw new InvalidOperationException();
            }
            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        public static Result Success()
        {
            return new(true, Error.None);
        }

        public static Result<TValue> Success<TValue>(TValue value)
        {
            return new(value, true, Error.None);
        }

        public static Result<TValue> Create<TValue>(TValue? value)
        {
            return new(value, true, Error.None);
        }

        public static Result<TValue> Failure<TValue>(Error Error)
        {
            return new(default, false, Error);
        }

        public static Result Failure(Error Error)
        {
            return new(false, Error);
        }

    }
}