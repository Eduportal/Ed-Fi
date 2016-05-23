namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System;
    using System.Xml.Schema;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class PriorDescriptorReference : ProcessChainOfResponsibilityBase
    {
        private static readonly Type _expectedType = typeof(string);

        private const string _annotationName = "Descriptor";
        private const string _expectedObjectName = "PriorDescriptor";

        public PriorDescriptorReference(IProcessChainOfResponsibility next)
                : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaObjectName == _expectedObjectName &&
                   schemaObject.XmlSchemaType.Name == "DescriptorReferenceType" &&
                   schemaObject.XmlSchemaTypeGroup == "Base" &&
                   !schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var schemaObjectTypeName = this.GetElementAnnotation(schemaObject.XmlSchemaObject as XmlSchemaElement, _annotationName);

            var expectedRestProperty = new ExpectedRestProperty
                                           {
                                                   ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                                   PropertyExpectedRestType = new ExpectedRestType
                                                                                  {
                                                                                          ClassName = _expectedType.Name,
                                                                                          Namespace = _expectedType.Namespace
                                                                                  },
                                                   PropertyName = _expectedObjectName
                                           };

            return new ReferenceTypeProcessResult
                       {
                               ProcessChildren = false,
                               ProcessingRuleName = "Prior Descriptor Reference",
                               ReferenceTypeName = schemaObjectTypeName,
                               Expected = expectedRestProperty
                       };
        }
    }
}