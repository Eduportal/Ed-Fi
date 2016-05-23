using System;
using System.Linq;

namespace EdFi.Ods.XsdToOdsMap
{
    using System.Collections.Generic;
    using System.Text;
    using System.Xml.Schema;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using EdFi.Ods.CodeGen.XsdToWebApi.Process;

    public class OutputParsedSchemaObject
    {
        public static List<string> ResultHeader()
        {
            return new List<string>
            {
               "Interchange", "Domain Entity", "XSD Path","XSD Element", "XSD Element Type", "Required/Optional", "Aggregate Resource", "Resource", "Property Name", "Property Type", "Length", "Description"
            };
        }

        public static List<string>[] ResultList(ParsedSchemaObject parsedSchemaObject)
        {
            var path = FormatPath(parsedSchemaObject);
            var pathSegments = path.Split('\\');
            var interchange = pathSegments[0];
            var domainEntity = pathSegments.Length > 1 ? pathSegments[1] : parsedSchemaObject.XmlSchemaObjectName;
            var aggregateResource = domainEntity;
            var objectName = parsedSchemaObject.XmlSchemaObjectName;
            var isAttribute = parsedSchemaObject.IsAttribute ? "[Attribute]" : String.Empty;
            var formattedSchemaType = FormatXmlSchemaType(parsedSchemaObject.XmlSchemaType, parsedSchemaObject.XmlSchemaTypeGroup);
            var formattedSchemaValidation = FormatSchemaValidation(parsedSchemaObject.SchemaValidations);
            var documentation = !String.IsNullOrEmpty(parsedSchemaObject.Documentation) ? parsedSchemaObject.Documentation : String.Empty;
            var elementBounds = FormatOptionalCollection(parsedSchemaObject.IsOptional, parsedSchemaObject.IsCollection);
            var length = FormatLength(parsedSchemaObject);
            var requiredOrOptional = parsedSchemaObject.IsOptional ? "Optional" : "Required";

            var resultList = new List<string> { interchange, domainEntity, path, String.Format(objectName, isAttribute), String.Format("{0}{1}{2}", formattedSchemaType, formattedSchemaValidation, elementBounds), requiredOrOptional, aggregateResource };

                                            
            resultList.AddRange(OutputProcessResult.ResultList(parsedSchemaObject.ProcessResult));
            resultList.Add(length);
            resultList.Add(documentation);


            var expectedMultipleRestProperties = parsedSchemaObject.ProcessResult.Expected as ExpectedMultipleRestProperties;
            if (expectedMultipleRestProperties == null)
                return new List<string>[] { resultList };

            var resultList2 = new List<string> { interchange, domainEntity, path, String.Format(objectName, isAttribute), String.Format("{0}{1}{2}", formattedSchemaType, formattedSchemaValidation, elementBounds), requiredOrOptional, aggregateResource };

            resultList2.AddRange(OutputProcessResult.ResultList(parsedSchemaObject.ProcessResult, expectedMultipleRestProperties.AdditionalProperty));
            resultList2.Add(length);
            resultList2.Add(documentation);
            return new List<string>[] { resultList, resultList2 };
        }
        
        private static string FormatPath(ParsedSchemaObject parentSchemaObject, bool showElement = false, StringBuilder path = null)
        {
            if(path == null)
                path = new StringBuilder();

            if (parentSchemaObject.ParentElement == null || parentSchemaObject is ParsedInterchange)
                return path.ToString();

            if (path.Length == 0)
            {
                path.Append(parentSchemaObject.ParentElement.XmlSchemaObjectName);
                if(showElement)
                    path.Append("\\").Append(parentSchemaObject.XmlSchemaObjectName);
            }
            else
                path.Insert(0, string.Format("{0}\\", parentSchemaObject.ParentElement.XmlSchemaObjectName));

            FormatPath(parentSchemaObject.ParentElement, showElement, path);

            return path.ToString();
        }

        private static string FormatOptionalCollection(bool isOptional, bool isCollection)
        {
            if (isOptional && isCollection)
                return "[0..*]";

            if (isCollection)
                return "[1..*]";

            if (isOptional)
                return "[0..1]";

            return String.Empty;
        }

        private static string FormatXmlSchemaType(XmlSchemaType schemaType, string schemaTypeGroup)
        {
            var complexType = schemaType as XmlSchemaComplexType;
            if (complexType != null)
                return complexType.Name;

            if (schemaTypeGroup == "Enumeration")
                return schemaType.Name;

            var simpleType = schemaType as XmlSchemaSimpleType;
            return simpleType.TypeCode.ToString();
        }

        private static string FormatSchemaValidation(IEnumerable<ISchemaValidation> schemaValidations)
        {
            var sb = new StringBuilder();
            foreach (var schemaValidation in schemaValidations)
            {
                sb.Append(schemaValidation);
            }
            return sb.ToString();
        }

        private static string FormatLength(ParsedSchemaObject parsedSchemaObject)
        {
            var maxLengthValidation = parsedSchemaObject.SchemaValidations.FirstOrDefault(x => x.GetType().Equals(typeof(MaxStringLength))) as MaxStringLength;
            var length = maxLengthValidation != null ? maxLengthValidation.MaxLength.ToString() : "";
            return length;
        }
    }
}
