namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class NoMatch : ProcessChainOfResponsibilityBase
    {
        public NoMatch(IProcessChainOfResponsibility next) : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return false;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            return new ProcessResult
                       {
                           ProcessChildren = false,
                           ProcessingRuleName = "No Match"
                       };
        }
    }
}