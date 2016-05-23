using System;
using System.Runtime.Serialization;

namespace EdFi.Ods.Common.Exceptions
{
    /// <summary>
    ///     Indicates that a resource or persistent model identified for an operation was not found.
    /// </summary>
    [Serializable]
    public class NotFoundException : Exception
    {
        public string TypeName { get; set; }
        public string Identifier { get; set; }
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public NotFoundException()
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string message, string typeName, string identifier) : base(message)
        {
            TypeName = typeName;
            Identifier = identifier;
        }

        public NotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}