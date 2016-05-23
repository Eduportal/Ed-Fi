// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

namespace EdFi.Ods.CodeGen
{
    using System.Xml.Linq;

    public static class XElementExtensions
    {
        public static string AttributeValue(this XElement element, XName name)
        {
            if (element == null)
                return null;

            return (string)element.Attribute(name);
        }
    }
}