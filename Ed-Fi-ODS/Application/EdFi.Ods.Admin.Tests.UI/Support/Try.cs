namespace EdFi.Ods.Admin.UITests.Support
{
    using System;
    using System.Threading;

    public static class Try
    {
        public static bool WaitingForThis(Func<bool> thingToDo, TimeSpan duration)
        {
            DateTime startTime = DateTime.Now;

            while (!thingToDo())
            {
                if (DateTime.Now - startTime > duration)
                    return false;

                Thread.Sleep(100);
            }

            return true;
        }
    }
}
