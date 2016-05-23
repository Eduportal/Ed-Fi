namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class Interchange : ProcessChainOfResponsibilityBase
    {
        public Interchange(IProcessChainOfResponsibility next)
                : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject is ParsedInterchange;

        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            return new ProcessResult
                       {
                           ProcessChildren = true,
                           ProcessingRuleName = "Interchange",
                           Expected = new Continue()
                       };
        }
    }
}