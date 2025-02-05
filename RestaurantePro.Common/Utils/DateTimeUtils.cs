using System;

namespace RestaurantePro.Common.Utils
{
    public static class DateTimeUtils
    {
        public static DateTime ToLocalTime(this DateTime utcDateTime, string timeZoneId = "America/Bogota")

        {

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }
    }
}