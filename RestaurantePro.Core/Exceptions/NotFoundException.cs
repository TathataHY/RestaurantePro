using System;

namespace RestaurantePro.Core.Exceptions
{
    public class NotFoundException : BusinessException
    {
        public NotFoundException(string message)
            : base(message, "NOT_FOUND")
        {
        }
    }
}