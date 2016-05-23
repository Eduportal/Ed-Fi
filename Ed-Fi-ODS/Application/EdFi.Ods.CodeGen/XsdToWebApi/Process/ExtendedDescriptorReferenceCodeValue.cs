using System;
using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{

    public class ExtendedDescriptorReferenceCodeValue : ProcessChainOfResponsibilityBase
    {
        private static readonly Type _expectedType = typeof(string);

        private const string _expectedElementName = "CodeValue";

        public ExtendedDescriptorReferenceCodeValue(IProcessChainOfResponsibility next) : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaObjectName == _expectedElementName &&
                   schemaObject.ParentElement.XmlSchemaTypeGroup == "Extended Descriptor Reference" &&
                   !schemaObject.ParentElement.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var parentContext = schemaObject.ParentElement.ProcessResult.Expected as ExpectedContext;

            var expectedRestProperty = new ExpectedTerminalRestProperty
                                           {
                                                   ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                                   PropertyExpectedRestType = new ExpectedRestType
                                                                                  {
                                                                                          ClassName = _expectedType.Name,
                                                                                          Namespace = _expectedType.Namespace
                                                                                  },
                                                   PropertyName = parentContext.Context,
                                                   PropertyType = _expectedType,
                                                   IsNullable = parentContext.ContextNullable
                                           };

            return new ProcessResult
                       {
                               ProcessChildren = false,
                               ProcessingRuleName = "Extended Descriptor Reference Code Value",
                               Expected = expectedRestProperty
                       };
        }
    }
}