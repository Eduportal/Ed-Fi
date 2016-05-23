namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System.Xml.Schema;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class SkipIdAttribute : ProcessChainOfResponsibilityBase
    {
        public SkipIdAttribute(IProcessChainOfResponsibility next) : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaObjectName == "id" &&
                   schemaObject.IsAttribute &&
                   schemaObject.XmlSchemaType.TypeCode == XmlTypeCode.Id;

        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            return new ProcessResult
                        {
                            ProcessChildren = false,
                            ProcessingRuleName = "Skip Id Attribute",
                            Expected = new Skip()
                        };
        }
    }
}
