using System;

namespace EdFi.Ods.Common.Utils.Extensions
{
    public static class IntExtensions
    {
         public static TimeSpan Seconds(this int seconds)
         {
             return TimeSpan.FromSeconds(seconds);
         }
    }
}