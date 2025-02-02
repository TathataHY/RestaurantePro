using System;

namespace RestaurantePro.Core.Exceptions
{
    public class BusinessException : Exception
    {
        public string ErrorCode { get; }

        public BusinessException(string message, string errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}