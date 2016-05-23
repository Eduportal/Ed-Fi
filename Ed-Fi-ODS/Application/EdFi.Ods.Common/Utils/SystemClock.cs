using System;

namespace EdFi.Ods.Common.Utils
{

    public static class SystemClock
    {
        static SystemClock()
        {
            UseUtcNow();
        }

        public static Func<DateTime> Now = () =>
        {
            throw new Exception(string.Format("{0} has not been initialized.", typeof(SystemClock)));
        };

        public static void UseUtcNow()
        {
            Now = () => DateTime.UtcNow;
        }
    }
}