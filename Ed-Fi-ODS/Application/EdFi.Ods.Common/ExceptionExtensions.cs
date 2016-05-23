using System;

namespace EdFi.Ods.Common
{
    public static class ExceptionExtensions
    {
        public static string GetAllMessages(this Exception ex, string delimiter = "\r\n")
        {
            string message = ex.Message;

            var currentException = ex.InnerException;

            while (currentException != null)
            {
                message += delimiter + currentException.Message;
                currentException = currentException.InnerException;
            }
            return message;
        }

        public static string GetAllStackTraces(this Exception ex)
        {
            //This method is here for possible customization in future, but for now, the built in ToString method works fine with what we want
            return ex.ToString();
        }
    }
}