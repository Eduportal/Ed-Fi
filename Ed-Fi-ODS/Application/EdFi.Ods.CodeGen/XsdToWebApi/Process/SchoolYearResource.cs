using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{

    public class SchoolYearResource : ProcessChainOfResponsibilityBase
    {
        private static readonly Type _expectedType = typeof(short);
        private static readonly Type _nullableExpectedType = typeof(short?);

        private const string _expectedSchemaType = "SchoolYearType";

        public SchoolYearResource(IProcessChainOfResponsibility next)
            : base(next)
        {
        }

        protected bool IsNestedExtendedReference(ParsedSchemaObject schemaObject)
        {
            if (schemaObject == null)
                return false;

            if (schemaObject.XmlSchemaTypeGroup == "Extended Reference")
                return true;

            return IsNestedExtendedReference(schemaObject.ParentElement);
        }

        protected bool IsReferenceWithoutForeignKey(ParsedSchemaObject schemaObject)
        {
            var elementName = schemaObject.XmlSchemaObjectName;
            var parentElementName = schemaObject.ParentElement.XmlSchemaObjectName;

            return NoForeignKeyMetadata.PredefinedNoForeignKeyMetadata.Any(x => x.ElementName == elementName && x.ParentElementName == parentElementName);
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaType.Name == _expectedSchemaType &&
                   schemaObject.XmlSchemaTypeGroup == "Enumeration" &&
                   !schemaObject.IsCollection &&
                   !IsNestedExtendedReference(schemaObject.ParentElement) &&
                   !IsReferenceWithoutForeignKey(schemaObject);
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var objectName = schemaObject.XmlSchemaObjectName;
            var context = this.GetContext(schemaObject);

            var isOptional = this.GetIsOptional(schemaObject);


            var expectedRestType = new ExpectedRestType
                                        {
                                            ClassName = "SchoolYearTypeReference",
                                            Namespace = BuildNamespace("SchoolYearType")
                                        };
            var expectedRestProperty = new ExpectedMultipleRestProperties
                                            {
                                                ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                                PropertyExpectedRestType = expectedRestType,
                                                PropertyName = context + "SchoolYearTypeReference",
                                                IsPropertyCollection = false,
                                                AdditionalProperty = new ExpectedTerminalRestProperty
                                                                        {
                                                                            ContainingExpectedRestType = expectedRestType,
                                                                            PropertyExpectedRestType = new ExpectedRestType
                                                                                                            {
                                                                                                                ClassName = isOptional ? _expectedType.Name + "?" : _expectedType.Name,
                                                                                                                Namespace = _expectedType.Namespace
                                                                                                            },
                                                                            PropertyName = "SchoolYear",
                                                                            PropertyType = _expectedType,
                                                                            IsNullable = false
                                                                        }
                                            };

            return new ProcessResult
                       {
                               ProcessChildren = false,
                               ProcessingRuleName = "School Year Resource",
                               Expected = expectedRestProperty
                       };
        }
    }
}