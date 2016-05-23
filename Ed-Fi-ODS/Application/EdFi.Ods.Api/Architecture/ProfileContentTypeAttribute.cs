// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************
using System;

namespace EdFi.Ods.Api.Architecture
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProfileContentTypeAttribute : Attribute
    {
        public string MediaTypeName { get; private set; }

        public ProfileContentTypeAttribute(string mediaTypeName)
        {
            MediaTypeName = mediaTypeName;
        }
    }
}