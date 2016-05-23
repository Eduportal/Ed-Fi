namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class SkipDescriptorReferenceNamespace : ProcessChainOfResponsibilityBase
    {
        public SkipDescriptorReferenceNamespace(IProcessChainOfResponsibility next)
                : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaObjectName == "Namespace" &&
                   schemaObject.ParentElement.XmlSchemaTypeGroup == "Extended Descriptor Reference";

        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            return new ProcessResult
                       {
                               ProcessChildren = false,
                               ProcessingRuleName = "Skip Descriptor Reference Namespace Attribute",
                               Expected = new Skip()
                       };
        }
    }
}