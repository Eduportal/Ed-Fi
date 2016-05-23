using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.CodeGen.XsdToWebApi.Parse;
using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    public class ExtendedReferenceResource : ProcessChainOfResponsibilityBase
    {
        private const string _expectedElementNameSuffix = "Reference";
        private const string _expectedTypeSuffix = "ReferenceType";

        public ExtendedReferenceResource(IProcessChainOfResponsibility next)
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
            return schemaObject.XmlSchemaTypeGroup == "Extended Reference" &&
                   !schemaObject.IsCollection &&
                   schemaObject.XmlSchemaObjectName.EndsWith(_expectedElementNameSuffix) &&
                   !IsNestedExtendedReference(schemaObject.ParentElement) &&
                   !IsReferenceWithoutForeignKey(schemaObject);
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var containingType = this.GetContainingType(schemaObject);
            string context;
            if (this.TryGetPredefinedContext(schemaObject, out context))
            {
                if (schemaObject.XmlSchemaObjectName.StartsWith(context))
                    context = string.Empty;
            }

            var containingExpectedContext = containingType as ExpectedContext;
            var contextNullable = schemaObject.IsOptional;
            if (containingExpectedContext != null)
            {
                contextNullable = containingExpectedContext.ContextNullable || schemaObject.IsOptional;
                context = containingExpectedContext.Context + context;
            }

            var schemaObjectTypeName = schemaObject.XmlSchemaType.Name.StripProjectPrefix();
            var referenceTypeName = schemaObjectTypeName.Substring(0, schemaObjectTypeName.Length - _expectedTypeSuffix.Length);

            var expectedRestProperty = new ExpectedRestProperty
                                            {
                                                ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                                PropertyExpectedRestType = new ExpectedRestType
                                                                                {
                                                                                    ClassName = referenceTypeName + "Reference",
                                                                                    Namespace = BuildNamespace(referenceTypeName)
                                                                                },
                                                PropertyName = context + schemaObject.XmlSchemaObjectName
                                            };

            return new ProcessResult
                       {
                            ProcessChildren = schemaObject.ParentElement.ProcessResult.Expected != null,
                            ProcessingRuleName = "Extended Reference Resource",
                            Expected = expectedRestProperty
                       };
        }
    }
}