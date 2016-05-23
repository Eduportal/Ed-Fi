using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    public class SkipLookup : ProcessChainOfResponsibilityBase
    {
        public SkipLookup(IProcessChainOfResponsibility next)
            : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaTypeGroup == "Lookup";
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            return new ProcessResult
            {
                ProcessChildren = false,
                ProcessingRuleName = "Skip Lookup",
                Expected = new Skip()
            };
        }
    }
}
