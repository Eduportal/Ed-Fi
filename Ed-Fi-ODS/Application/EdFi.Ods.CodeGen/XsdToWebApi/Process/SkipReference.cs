namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System.Collections.Generic;
    using System.Linq;

    using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public partial class SkipReference : ProcessChainOfResponsibilityBase
    {
        public SkipReference(IProcessChainOfResponsibility next) : base(next){}

        private bool MatchingSchemaObject(ParsedSchemaObject schemaObject, IEnumerable<string> path)
        {
            if (!path.Any())
                return true;

            var normalizedName = string.Empty;
            if (!string.IsNullOrEmpty(schemaObject.XmlSchemaType.Name))
                normalizedName = schemaObject.XmlSchemaType.Name.StripProjectPrefix().StripExtensionNameValues();

            return path.First() == normalizedName && this.MatchingSchemaObject(schemaObject.ParentElement, path.Skip(1));
        }


        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return !schemaObject.IsCollection &&
                   this._skipReferences.Any(x => this.MatchingSchemaObject(schemaObject, x));
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            return new ProcessResult
                       {
                               ProcessChildren = false,
                               ProcessingRuleName = "Skip Reference",
                               Expected = new Skip()
                       };
        }
    }
}