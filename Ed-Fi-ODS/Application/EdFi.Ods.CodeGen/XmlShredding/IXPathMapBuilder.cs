namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System;
    using System.Collections.Generic;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public interface IXPathMapBuilder
    {
        IMapStep DeserializeMap(string serializedMap);
        IEnumerable<Tuple<ParsedSchemaObject, string>> BuildStartingXsdElementSerializedMapTuplesFor(ParsedSchemaObject parentXsd);
    }
}