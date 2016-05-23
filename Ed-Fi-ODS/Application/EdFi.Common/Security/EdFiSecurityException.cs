// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Runtime.Serialization;

namespace EdFi.Common.Security
{
    [Serializable]
    public class EdFiSecurityException : Exception
    {
        public EdFiSecurityException()
        {
        }

        public EdFiSecurityException(string message) : base(message)
        {
        }

        public EdFiSecurityException(string message, string resource, string action) : base(message)
        {
            Resource = resource;
            Action = action;
        }

        public EdFiSecurityException(string message, string resource, string action, string apiKey) : base(message)
        {
            Resource = resource;
            Action = action;
            ApiKey = apiKey;
        }

        public EdFiSecurityException(string message, string resource, string action, string apiKey, Exception inner) : base(message, inner)
        {
            Resource = resource;
            Action = action;
            ApiKey = apiKey;
        }

        public EdFiSecurityException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EdFiSecurityException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public string ApiKey { get; private set; }

        public string Resource { get; private set; }

        public string Action { get; private set; }
    }
}