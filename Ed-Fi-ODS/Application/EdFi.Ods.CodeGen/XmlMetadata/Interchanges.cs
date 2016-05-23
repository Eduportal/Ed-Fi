using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.CodeGen.XmlShredding;
using EdFi.Ods.CodeGen.XsdToWebApi;


namespace EdFi.Ods.Metadata
{
    using System.IO;
    public static class Interchanges
    {
        public static IEnumerable<Interchange> GetInterchanges(string schemaDirectory)
        {
            var retVal = new List<Interchange>();

            var xsdPaths = Directory.GetFiles(schemaDirectory, InterchangeXsdFileProvider.SearchPattern).ToArray(); 

            foreach (var xsdFile in xsdPaths)
            {
                var loader = InterchangeLoader.Load(xsdFile);
                foreach (var interchange in loader)
                {
                    var elements = new List<Element>();
                    
                    foreach (var elementName in interchange.ChildElements.Select(x => x.XmlSchemaObjectName))
                    {
                        elements.Add(new Element {Name = elementName, Type = elementName});
                    }

                    retVal.Add(new Interchange {Name = interchange.XmlSchemaObjectName.Substring(11), Elements = elements.ToArray()});
                }
            }

            return retVal;
        }
    }
}
