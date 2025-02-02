using System;
using System.Collections.Generic;

namespace RestaurantePro.Core.Exceptions
{
    public class ValidationException : BusinessException
    {
        public IEnumerable<string> Errors { get; }

        public ValidationException(string message, IEnumerable<string> errors)
            : base(message, "VALIDATION_ERROR")
        {
            Errors = errors;
        }

        public ValidationException(string message)
            : base(message, "VALIDATION_ERROR")
        {
            Errors = new List<string> { message };
        }
    }
}