using System;
using System.Threading.Tasks;

namespace EdFi.Ods.WebService.Tests.Extensions
{
    public static class TaskExtensions
    {
        public const int DefaultHttpRequestTimeoutSeconds = 60;

        public static T Sync<T>(this Task<T> task, int timeoutSeconds = DefaultHttpRequestTimeoutSeconds)
        {
            try
            {
                task.Wait(TimeSpan.FromSeconds(timeoutSeconds));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            if (task.IsCanceled)
                throw new Exception("Async test failure! Request was canceled");

            if (!task.IsCompleted)
                throw new Exception("Async test failure! Request timed out.");

            return task.Result;
        }
    }
}