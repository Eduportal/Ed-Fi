// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;

namespace EdFi.Ods.Api.Services.Metadata
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class SwaggerMetadataResourceAttribute : Attribute
    {
        public SwaggerMetadataResourceAttribute(string baseName)
        {
            BaseName = baseName;
        }

        public string BaseName { get; private set; }
    }
}