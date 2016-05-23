namespace EdFi.Ods.XsdParsing.Tests.InterchangeToAggregate
{
    using System.Collections.Generic;
    using System.Text;
    using System.Xml.Schema;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class OutputParsedSchemaObject
    {
        public static string FormattedResult(ParsedSchemaObject parsedSchemaObject)
        {
            var parentTypeName = string.Empty;
            var parentInterchange = parsedSchemaObject.ParentElement as ParsedInterchange;
            if (parentInterchange != null)
                parentTypeName = parentInterchange.InterchangeName;
            else if (parsedSchemaObject.ParentXmlSchemaType != null)
                parentTypeName = parsedSchemaObject.ParentXmlSchemaType.Name;

            var objectName = parsedSchemaObject.XmlSchemaObjectName;
            var isAttribute = parsedSchemaObject.IsAttribute ? "[Attribute]" : string.Empty;
            var formattedSchemaType = FormatXmlSchemaType(parsedSchemaObject.XmlSchemaType, parsedSchemaObject.XmlSchemaTypeGroup);
            var formattedSchemaValidation = FormatSchemaValidation(parsedSchemaObject.SchemaValidations);
            var elementBounds = FormatOptionalCollection(parsedSchemaObject.IsOptional, parsedSchemaObject.IsCollection);

            return string.Format("{0}\t{1}{2}\t{3}{4}{5}\t{6}\t{7}", parentTypeName, objectName, isAttribute, formattedSchemaType, formattedSchemaValidation, elementBounds, OutputProcessResult.FormattedResult(parsedSchemaObject.ProcessResult), OutputProcessResultCorrectness.FormattedResult(parsedSchemaObject.ProcessResult));
        }

        protected static string FormatOptionalCollection(bool isOptional, bool isCollection)
        {
            if (isOptional && isCollection)
                return "[0..*]";

            if (isCollection)
                return "[1..*]";

            if (isOptional)
                return "[0..1]";

            return string.Empty;
        }

        protected static string FormatXmlSchemaType(XmlSchemaType schemaType, string schemaTypeGroup)
        {
            var complexType = schemaType as XmlSchemaComplexType;
            if (complexType != null)
                return complexType.Name;

            if (schemaTypeGroup == "Enumeration")
                return schemaType.Name;

            var simpleType = schemaType as XmlSchemaSimpleType;
            return simpleType.TypeCode.ToString();
        }

        protected static string FormatSchemaValidation(IEnumerable<ISchemaValidation> schemaValidations)
        {
            var sb = new StringBuilder();
            foreach (var schemaValidation in schemaValidations)
            {
                sb.Append(schemaValidation);
            }

            return sb.ToString();
        }
    }
}
