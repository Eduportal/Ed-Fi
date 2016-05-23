namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System;

    using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class ExtendedDescriptorReferenceCollectionCodeValue : ProcessChainOfResponsibilityBase
    {
        private static readonly Type _expectedType = typeof(string);

        private const string _expectedElementName = "CodeValue";
        private const string _expectedTypeSuffix = "DescriptorReferenceType";
        private const string _expectedRestPropertySuffix = "Descriptor";

        public ExtendedDescriptorReferenceCollectionCodeValue(IProcessChainOfResponsibility next) : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaObjectName == _expectedElementName &&
                   schemaObject.ParentElement.XmlSchemaTypeGroup == "Extended Descriptor Reference" &&
                   schemaObject.ParentElement.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var schemaObjectTypeName = schemaObject.ParentElement.XmlSchemaType.Name.StripProjectPrefix();
            var baseDescriptorName = schemaObjectTypeName;
            if (baseDescriptorName.EndsWith(_expectedTypeSuffix))
                baseDescriptorName = baseDescriptorName.Substring(0, baseDescriptorName.Length - _expectedTypeSuffix.Length);

            var expectedRestProperty = new ExpectedTerminalRestProperty
                                           {
                                                   ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                                   PropertyExpectedRestType = new ExpectedRestType
                                                                                  {
                                                                                          ClassName = _expectedType.Name,
                                                                                          Namespace = _expectedType.Namespace
                                                                                  },
                                                   PropertyName = baseDescriptorName + _expectedRestPropertySuffix,
                                                   PropertyType = _expectedType,
                                                   IsNullable = false
                                           };

            return new ProcessResult
                       {
                               ProcessChildren = true,
                               ProcessingRuleName = "Extended Descriptor Reference Collection Code Value",
                               Expected = expectedRestProperty
                       };
        }
    }
}