namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class SkipTopLevelReference : ProcessChainOfResponsibilityBase
    {
        public SkipTopLevelReference(IProcessChainOfResponsibility next) : base(next) { }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaTypeGroup == "Extended Reference" &&
                   schemaObject.ParentElement is ParsedInterchange;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            return new ProcessResult
                       {
                               ProcessChildren = false,
                               ProcessingRuleName = "Skip Top Level Reference",
                               Expected = new Skip()
                       };
        }
    }
}