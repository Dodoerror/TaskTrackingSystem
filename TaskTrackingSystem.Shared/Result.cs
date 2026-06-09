using System;

namespace TaskTrackingSystem.Shared
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public T? Value { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; } // 200, 400, 404, etc.

        public static Result<T> Success(T value, int statusCode = 200)
        {
            return new Result<T>
            {
                IsSuccess = true,
                Value = value,
                StatusCode = statusCode
            };
        }

        public static Result<T> Failure(string errorMessage, int statusCode = 400)
        {
            return new Result<T>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                StatusCode = statusCode
            };
        }
    }

    public class Result
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }

        public static Result Success(int statusCode = 200)
        {
            return new Result
            {
                IsSuccess = true,
                StatusCode = statusCode
            };
        }

        public static Result Failure(string errorMessage, int statusCode = 400)
        {
            return new Result
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                StatusCode = statusCode
            };
        }
    }
}
