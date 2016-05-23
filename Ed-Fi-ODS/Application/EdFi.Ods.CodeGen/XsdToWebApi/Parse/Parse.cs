using System;
using System.Linq;
using System.Xml.Schema;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Parse
{
    public class Parse
    {
        public ParsedSchemaElement ParseSchemaElement(XmlSchemaElement schemaElement, XmlSchemaType schemaType)
        {
            var declaringSchemaType = this.GetDeclaringSchemaType(schemaElement.Parent);

            var documentation = GetDocumentation(schemaElement);
            if (string.IsNullOrWhiteSpace(documentation))
                documentation = GetDocumentation(schemaElement.ElementSchemaType);

            return new ParsedSchemaElement
                       {
                               XmlSchemaElement = schemaElement,
                               XmlSchemaType = schemaElement.ElementSchemaType,
                               ParentXmlSchemaType = schemaType,
                               DeclaringSchemaType = declaringSchemaType,
                               XmlSchemaTypeGroup = this.GetTypeGroupAnnotation(schemaElement.ElementSchemaType),
                               IsComplexType = schemaElement.ElementSchemaType is XmlSchemaComplexType,
                               IsCollection = this.IsCollection(schemaElement),
                               IsOptional = this.IsOptional(schemaElement),
                               IsAttribute = false,
                               SchemaValidations = this.GetSchemaValidations(schemaElement.ElementSchemaType),
                               Documentation = documentation,
                       };
        }

        public ParsedSchemaAttribute ParseSchemaAttribute(XmlSchemaAttribute schemaAttribute, XmlSchemaType schemaType)
        {
            var declaringSchemaType = this.GetDeclaringSchemaType(schemaAttribute.Parent);

            return new ParsedSchemaAttribute
                       {
                               XmlSchemaAttribute = schemaAttribute,
                               XmlSchemaType = schemaAttribute.AttributeSchemaType,
                               ParentXmlSchemaType = schemaType,
                               DeclaringSchemaType = declaringSchemaType,
                               XmlSchemaTypeGroup = this.GetTypeGroupAnnotation(schemaAttribute.AttributeSchemaType),
                               IsComplexType = false,
                               IsCollection = false,
                               IsOptional = this.IsOptional(schemaAttribute),
                               IsAttribute = true,
                               SchemaValidations = this.GetSchemaValidations(schemaAttribute.AttributeSchemaType),
                               Documentation = GetDocumentation(schemaAttribute)
                       };

        }

        public ParsedInterchange ParsedInterchangeElement(XmlSchemaElement schemaElement)
        {
            return new ParsedInterchange
                       {
                            InterchangeName = schemaElement.Name,
                            XmlSchemaElement = schemaElement,
                            XmlSchemaType = schemaElement.ElementSchemaType,
                            IsComplexType = schemaElement.ElementSchemaType is XmlSchemaComplexType,
                            IsCollection = this.IsCollection(schemaElement),
                            IsOptional = this.IsOptional(schemaElement),
                            IsAttribute = false,
                            SchemaValidations = this.GetSchemaValidations(schemaElement.ElementSchemaType),
                            Documentation = GetDocumentation(schemaElement)
                       };
        }

        protected XmlSchemaComplexType GetDeclaringSchemaType(XmlSchemaObject parent)
        {
            var complexType = parent as XmlSchemaComplexType;
            if (complexType == null && parent == null)
                return null;
            if (complexType == null)
                return this.GetDeclaringSchemaType(parent.Parent);

            return complexType;
        }

        protected bool IsOptional(XmlSchemaElement schemaElement)
        {
            if (schemaElement.MinOccurs != 0 && schemaElement.MinOccurs != 1)
                throw new ApplicationException("Min occurs greater than 1");

            if (schemaElement.MinOccurs == 0)
                return true;

            var parentChoice = schemaElement.Parent as XmlSchemaChoice;
            if (parentChoice == null)
                return false;

            return parentChoice.MinOccurs == 0;
        }

        protected bool IsOptional(XmlSchemaAttribute schemaAttribute)
        {
            return schemaAttribute.Use == XmlSchemaUse.Optional;
        }

        protected bool IsCollection(XmlSchemaElement schemaElement)
        {
            return schemaElement.MaxOccurs > 1;
        }

        protected ISchemaValidation[] GetSchemaValidations(XmlSchemaType schemaType)
        {
            var simpleType = schemaType as XmlSchemaSimpleType;
            if (simpleType == null)
                return new ISchemaValidation[0];

            if (simpleType.TypeCode == XmlTypeCode.String || simpleType.TypeCode == XmlTypeCode.Text || simpleType.TypeCode == XmlTypeCode.Duration)
                return new ISchemaValidation[]
                       {
                            MaxStringLength.GetMaxStringLength(simpleType)
                       };

            if (simpleType.TypeCode == XmlTypeCode.Decimal)
                return new ISchemaValidation[]
                       {
                           DecimalPrecision.GetDecimalPrecision(simpleType)
                       };

            return new ISchemaValidation[0];

        }

        protected string GetTypeGroupAnnotation(XmlSchemaType schemaType)
        {
            if (schemaType.Name == null && schemaType.Annotation == null)
                return null;

            foreach (var item in schemaType.Annotation.Items)
            {
                var annotationElement = item as XmlSchemaAppInfo;
                if (annotationElement != null)
                {
                    foreach (var xmlNode in annotationElement.Markup)
                    {
                        if (xmlNode.NamespaceURI == "http://ed-fi.org/annotation" && xmlNode.LocalName == "TypeGroup")
                            return xmlNode.InnerText;
                    }
                }
            }

            throw new ApplicationException("Unknown Type Group " + schemaType.Name);
        }

        protected string GetDocumentation(XmlSchemaAnnotated schemaAnnotated)
        {
            if (schemaAnnotated.Annotation == null)
                return null;

            foreach (var item in schemaAnnotated.Annotation.Items)
            {
                var annotationElement = item as XmlSchemaDocumentation;
                if (annotationElement != null && annotationElement.Markup.Length > 0)
                {
                    return annotationElement.Markup.Select(x => x.InnerText).Aggregate((y, z) => y + z);
                }
            }

            return null;
        }
    }
}