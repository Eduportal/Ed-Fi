using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace EdFi.Ods.XmlShredding
{
    public interface INodeSearch
    {
        XElement FindForeignKeyElement(XElement startingElement, string serializedRefMap);
    }
}