using System;

namespace Intilaqah.Infrastructure.Integrations.DTOs
{
    /// <summary>
    /// Standard result wrapper for all integration calls.
    /// Prevents integration failures from throwing exceptions into business logic.
    /// </summary>
    public class IntegrationResult
    {
        public bool    IsSuccess    { get; set; }
        public string? ErrorMessage { get; set; }
        public int     HttpStatus   { get; set; }
        public Guid?   LogId        { get; set; }
        // reference to IntegrationLog entry

        public static IntegrationResult Success(Guid? logId = null)
            => new() { IsSuccess = true, LogId = logId };

        public static IntegrationResult Failure(
            string error, int httpStatus = 0, Guid? logId = null)
            => new()
            {
                IsSuccess    = false,
                ErrorMessage = error,
                HttpStatus   = httpStatus,
                LogId        = logId,
            };
    }

    public class IntegrationResult<T> : IntegrationResult
    {
        public T? Data { get; set; }

        public static IntegrationResult<T> Success(T data, Guid? logId = null)
            => new() { IsSuccess = true, Data = data, LogId = logId };

        public static new IntegrationResult<T> Failure(
            string error, int httpStatus = 0, Guid? logId = null)
            => new()
            {
                IsSuccess    = false,
                ErrorMessage = error,
                HttpStatus   = httpStatus,
                LogId        = logId,
            };
    }
}
