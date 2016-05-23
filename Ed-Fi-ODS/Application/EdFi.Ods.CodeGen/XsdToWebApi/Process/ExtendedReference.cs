namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class ExtendedReference : ProcessChainOfResponsibilityBase
    {
        private const string _expectedTypeSuffix = "Reference";

        public ExtendedReference(IProcessChainOfResponsibility next)
                : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaTypeGroup == "Extended Reference" &&
                   !schemaObject.IsCollection &&
                   schemaObject.XmlSchemaObjectName.EndsWith(_expectedTypeSuffix);
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var containingType = this.GetContainingType(schemaObject);
            var context = this.GetContext(schemaObject);

            var containingExpectedContext = containingType as ExpectedContext;
            var contextNullable = schemaObject.IsOptional;
            if (containingExpectedContext != null)
            {
                contextNullable = containingExpectedContext.ContextNullable || schemaObject.IsOptional;
                context = containingExpectedContext.Context + context;
            }

            var expectedContext = new ExpectedContext(containingType)
                                    {
                                            Context = context,
                                            ContextNullable = contextNullable
                                    };

            return new ProcessResult
                       {
                            ProcessChildren = schemaObject.ParentElement.ProcessResult.Expected != null,
                            ProcessingRuleName = "Extended Reference",
                            Expected = expectedContext
                       };
        }
    }
}