using System;
using System.Net;

namespace EdFi.Ods.Swagger.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ApiResponseMessageAttribute : Attribute
    {
        private string _message;

        public HttpStatusCode Code { get; set; }

        public string Message
        {
            get
            {
                return _message ?? GetMessageFromCode(Code);
            }
            set { _message = value; }
        }

        public Type ResponseModel { get; set; }

        public ApiResponseMessageAttribute()
        {
            Code = HttpStatusCode.OK;
        }

        /// <summary>
        /// Generate a default message directly from an Http Status Code
        /// </summary>
        /// <param name="code">Http Status Code</param>
        /// <returns>the default message</returns>
        private string GetMessageFromCode(HttpStatusCode code)
        {
            return Enum.GetName(typeof(HttpStatusCode), code);
        }
    }
}
