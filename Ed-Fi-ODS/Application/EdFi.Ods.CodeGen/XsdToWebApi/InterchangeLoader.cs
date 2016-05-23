
namespace EdFi.Ods.CodeGen.XsdToWebApi
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Schema;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;

    public class InterchangeLoader
    {
        private const string _edfiNamespace = "http://ed-fi.org/0200";
        private static readonly Parse.Parse _parse = new Parse.Parse();

        public static List<ParsedSchemaObject> Load(string xsdPath, string interchangeFileName)
        {
            return Load(Path.Combine(xsdPath, interchangeFileName));
        }

        public static List<ParsedSchemaObject> Load(string schemaFile)
        {
            var parsedInterchanges = new List<ParsedSchemaObject>();
            // TODO: need a better way to determine if a file is an interchange file
            if (!schemaFile.Contains("Interchange"))
                return parsedInterchanges;

            var schemaSet = new XmlSchemaSet();
            schemaSet.Add(_edfiNamespace, schemaFile);
            schemaSet.Compile();

            foreach (XmlSchema schema in schemaSet.Schemas())
            {
                var xmlSchema = schema;

                if (xmlSchema.Elements.Count == 0)
                    continue;
                
                foreach (XmlSchemaElement element in xmlSchema.Elements.Values)
                {
                    var interchangeElement = _parse.ParsedInterchangeElement(element);
                    parsedInterchanges.Add(interchangeElement);

                    // Get the complex type of the element.
                    var complexType = element.ElementSchemaType as XmlSchemaComplexType;
                    var sequence = complexType.ContentTypeParticle as XmlSchemaGroupBase;
                    IterateSchemaObject(sequence.Items, complexType, interchangeElement);
                }
            }

            foreach (var parsedSchemaObject in parsedInterchanges)
            {
                Process.Process.ProcessSchemaObject(parsedSchemaObject);
                parsedSchemaObject.IsExtension = schemaFile.IsExtensionSchema();
            }

            return parsedInterchanges;

        }

        private static void IterateSchemaObject(XmlSchemaObjectCollection schemaObjects, XmlSchemaType contextType, ParsedSchemaObject complexTypeParsed)
        {
            foreach (XmlSchemaObject schemaObject in schemaObjects)
            {
                var schemaElement = schemaObject as XmlSchemaElement;
                if (schemaElement != null)
                {
                    var parsed = _parse.ParseSchemaElement(schemaElement, contextType);
                    complexTypeParsed.ChildElements.Add(parsed);
                    parsed.ParentElement = complexTypeParsed;

                    var complexType = schemaElement.ElementSchemaType as XmlSchemaComplexType;
                    if (complexType != null)
                        ProcessComplexType(complexType, parsed);
                }

                var schemaGroup = schemaObject as XmlSchemaGroupBase;
                if (schemaGroup != null)
                    IterateSchemaObject(schemaGroup.Items, contextType, complexTypeParsed);
            }
        }

        private static void ProcessComplexType(XmlSchemaComplexType complexType, ParsedSchemaObject complexTypeParsed)
        {
            // process attributes
            foreach (XmlSchemaAttribute attributeUse in complexType.AttributeUses.Values)
            {
                var parsed = _parse.ParseSchemaAttribute(attributeUse, complexType);
                complexTypeParsed.ChildElements.Add(parsed);
                parsed.ParentElement = complexTypeParsed;
            }
            
            // Get the sequence particle of the complex type.
            var sequence = complexType.ContentTypeParticle as XmlSchemaGroupBase;
            if (sequence != null)
                IterateSchemaObject(sequence.Items, complexType, complexTypeParsed);
        }
    }
}
