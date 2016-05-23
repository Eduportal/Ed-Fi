namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System.Collections.Generic;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class SkipType : ProcessChainOfResponsibilityBase
    {
        // TODO: load this from xml metadata
        private readonly List<string> _skipTypes = new List<string>
                                                       {
                                                               "StateEducationAgencyFederalFunds",
                                                               "LocalEducationAgencyFederalFunds",
                                                               //"BellSchedule"
                                                       };

        public SkipType(IProcessChainOfResponsibility next)
                : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return this._skipTypes.Contains(schemaObject.XmlSchemaType.Name);
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            return new ProcessResult
                       {
                           ProcessChildren = false,
                           ProcessingRuleName = "Skip Type",
                           Expected = new Skip()
                       };
        }
    }
}