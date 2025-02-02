using System;

namespace RestaurantePro.Common.Exceptions
{
    public class BusinessException : Exception
    {
        public string Code { get; }

        public BusinessException(string message, string code) : base(message)
        {
            Code = code;
        }
    }
}