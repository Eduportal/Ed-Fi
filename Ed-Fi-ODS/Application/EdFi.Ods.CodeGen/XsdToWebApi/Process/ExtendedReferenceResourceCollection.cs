using System.Collections.Generic;
using System.Linq;
using EdFi.Common.Inflection;
using EdFi.Ods.CodeGen.XsdToWebApi.Parse;
using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    public class ExtendedReferenceResourceCollection : ProcessChainOfResponsibilityBase
    {
        private const string _expectedElementNameSuffix = "Reference";
        private const string _expectedTypeSuffix = "ReferenceType";

        public ExtendedReferenceResourceCollection(IProcessChainOfResponsibility next)
                : base(next)
        {
        }

        protected bool IsReferenceWithoutForeignKey(ParsedSchemaObject schemaObject)
        {
            if (schemaObject.ParentElement == null)
                return false;

            var elementName = schemaObject.XmlSchemaObjectName;
            var parentElementName = schemaObject.ParentElement.XmlSchemaObjectName;

            return NoForeignKeyMetadata.PredefinedNoForeignKeyMetadata.Any(x => x.ElementName == elementName && x.ParentElementName == parentElementName);
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaTypeGroup == "Extended Reference" &&
                   schemaObject.IsCollection &&
                   schemaObject.XmlSchemaObjectName.EndsWith(_expectedElementNameSuffix) &&
                   !IsReferenceWithoutForeignKey(schemaObject);
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var parentTypeName = schemaObject.GetParentTypeName();

            var propertyTypeName = schemaObject.XmlSchemaObjectName;
            if (propertyTypeName.EndsWith(_expectedElementNameSuffix))
                propertyTypeName = propertyTypeName.Substring(0, propertyTypeName.Length - _expectedElementNameSuffix.Length);
            
            propertyTypeName = this.AddPropertyContext(schemaObject, propertyTypeName);
            propertyTypeName = parentTypeName + propertyTypeName;

            if (schemaObject.ParentElement.IsCommonSchemaTypeGroup())
            {
                var parentParentTypeName = schemaObject.ParentElement.GetParentTypeName();
                if (!propertyTypeName.StartsWith(parentParentTypeName))
                    propertyTypeName = parentParentTypeName + propertyTypeName;
            }

            var propertyName = CompositeTermInflector.MakePlural(propertyTypeName);


            var schemaObjectTypeName = schemaObject.XmlSchemaType.Name.StripProjectPrefix();
            var referenceTypeName = schemaObjectTypeName.Substring(0, schemaObjectTypeName.Length - _expectedTypeSuffix.Length);
            var expectedRestType = new ExpectedRestType
                                                                                {
                                                                                    ClassName = propertyTypeName,
                                                                                    Namespace = this.GetNamespace(schemaObject)
                                                                                };
            var expectedRestProperty = new ExpectedMultipleRestProperties
                                            {
                                                ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                                PropertyExpectedRestType = expectedRestType,
                                                PropertyName = propertyName,
                                                IsPropertyCollection = true,
                                                AdditionalProperty = new ExpectedRestProperty
                                                                            {
                                                                                ContainingExpectedRestType = expectedRestType,
                                                                                PropertyExpectedRestType = new ExpectedRestType
                                                                                                                {
                                                                                                                    ClassName = referenceTypeName + "Reference",
                                                                                                                    Namespace = BuildNamespace(referenceTypeName)
                                                                                                                },
                                                                                PropertyName = referenceTypeName + "Reference"
                                                                            }
                                            };

            return new ProcessResult
                       {
                               ProcessChildren = true,
                               ProcessingRuleName = "Extended Reference Resource Collection",
                               Expected = expectedRestProperty
                       };
        }
    }
}