using System;

namespace EdFi.Ods.Common
{
    public static class DateTimeExtensions
    {
        public static string ToETag(this DateTime value)
        {
            return value.ToBinary().ToString();
        }
    }
}