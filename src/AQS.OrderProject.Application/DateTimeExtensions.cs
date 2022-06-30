using System;
using System.Globalization;

namespace TFA.AQS.Order.Application
{
    public static class DateTimeExtensions
    {
        public static DateTime ToGmt0Time(this DateTime ukCreatedTimeUtc)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("GMT+0");
            var cstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.ParseExact(ukCreatedTimeUtc.ToString(CultureInfo.InvariantCulture), "yyyy-MM-dd'T'HH:mm:ss", null), cstZone);

            return cstTime;
        }

        public static DateTime ToGmt4Time(this DateTime ukCreatedTimeUtc)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("GMT-4");
            var cstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.ParseExact(ukCreatedTimeUtc.ToString(CultureInfo.InvariantCulture), "yyyy-MM-dd'T'HH:mm:ss", null), cstZone);

            return cstTime;
        }
    }
}