namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class DescriptorEnumeration : ProcessChainOfResponsibilityBase
    {
        private static readonly Type _expectedType = typeof(string);
        private const string _expectedElementSuffix = "Map";
        private const string _expectedPropertySuffix = "Type";

        public DescriptorEnumeration(IProcessChainOfResponsibility next) : base(next) {}

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaTypeGroup == "Enumeration" &&
                   schemaObject.ParentElement.XmlSchemaTypeGroup == "Descriptor" &&
                   !schemaObject.IsComplexType &&
                   !schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var schemaObjectTypeName = schemaObject.XmlSchemaObjectName;
            if (schemaObjectTypeName.EndsWith(_expectedElementSuffix))
                schemaObjectTypeName = schemaObjectTypeName.Substring(0, schemaObjectTypeName.Length - _expectedElementSuffix.Length);
            if (!schemaObjectTypeName.EndsWith(_expectedPropertySuffix))
                schemaObjectTypeName = schemaObjectTypeName + _expectedPropertySuffix;

            var expectedRestProperty = new ExpectedRestProperty
                                           {
                                                   ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                                   PropertyExpectedRestType = new ExpectedRestType
                                                                                  {
                                                                                          ClassName = _expectedType.Name,
                                                                                          Namespace = _expectedType.Namespace
                                                                                  },
                                                   PropertyName = this.AddPropertyContext(schemaObject, schemaObjectTypeName),
                                           };
            return new ProcessResult
                       {
                               ProcessChildren = false,
                               ProcessingRuleName = "Descriptor Enumeration",
                               Expected = expectedRestProperty
                       };
        }
    }
}