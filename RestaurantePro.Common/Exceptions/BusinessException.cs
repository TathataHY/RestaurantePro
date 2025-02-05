using System;

namespace RestaurantePro.Common.Exceptions
{
    public class BusinessException : Exception
    {
        public string Code { get; }
        public object[] Args { get; }

        public BusinessException(string message, string code = null, params object[] args)
            : base(message)
        {
            Code = code;
            Args = args;
        }
    }
}