namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System.Collections.Generic;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class SkipElement : ProcessChainOfResponsibilityBase
    {
        // TODO: load this from xml metadata
        private readonly List<string> _skipElements = new List<string>
                                                       {
                                                               "PriorDescriptor",
                                                       };

        public SkipElement(IProcessChainOfResponsibility next) : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return this._skipElements.Contains(schemaObject.XmlSchemaObjectName);
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            return new ProcessResult
                       {
                               ProcessChildren = false,
                               ProcessingRuleName = "Skip Element",
                               Expected = new Skip()
                       };
        }
    }
}
