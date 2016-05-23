namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System.Xml.Schema;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class SkipRefAttribute : ProcessChainOfResponsibilityBase
    {
        public SkipRefAttribute(IProcessChainOfResponsibility next)
                : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaObjectName == "ref" &&
                   schemaObject.IsAttribute &&
                   schemaObject.XmlSchemaType.TypeCode == XmlTypeCode.Idref;

        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            return new ProcessResult
                        {
                            ProcessChildren = false,
                            ProcessingRuleName = "Skip Ref Attribute",
                            Expected = new Skip()
                        };
        }
    }
}