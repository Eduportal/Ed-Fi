using System;
using System.Runtime.Serialization;

namespace EdFi.Common.Security.Claims
{
    [Serializable]
    public class AuthorizationContextException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public AuthorizationContextException() {}
        public AuthorizationContextException(string message) : base(message) {}
        public AuthorizationContextException(string message, Exception inner) : base(message, inner) {}

        protected AuthorizationContextException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {}
    }
}