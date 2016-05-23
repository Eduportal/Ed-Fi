namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class Identity : ProcessChainOfResponsibilityBase
    {
        private const string _expectedTypeSuffix = "Identity";

        public Identity(IProcessChainOfResponsibility next)
            : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaTypeGroup == "Identity" &&
                   !schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var containingType = this.GetContainingType(schemaObject);

            var context = containingType as ExpectedContext;
            if (context == null) // this happens when it is an extended reference collection
            {
                var contextString = this.GetContext(schemaObject.ParentElement);
                if (string.IsNullOrEmpty(contextString) && this.IsRecursiveIdentity(schemaObject))
                    contextString = this.GetRecursiveIdentityContext(schemaObject);

                context = new ExpectedContext(containingType)
                              {
                                      ContextNullable = false,
                                      Context = contextString
                              };
            }

            return new ProcessResult
                       {
                           ProcessChildren = schemaObject.ParentElement.ProcessResult.Expected != null,
                           ProcessingRuleName = "Identity",
                           Expected = context
                       };
        }

        private string GetRecursiveIdentityContext(ParsedSchemaObject schemaObject)
        {
            if (schemaObject == null)
                return string.Empty;

            if (schemaObject.ProcessResult == null)
                return this.GetRecursiveIdentityContext(schemaObject.ParentElement);

            var parentContext = schemaObject.ProcessResult.Expected as ExpectedContext;
            if (parentContext == null)
                return this.GetRecursiveIdentityContext(schemaObject.ParentElement);
            return parentContext.Context;
        }

        private bool IsRecursiveIdentity(ParsedSchemaObject schemaObject)
        {
            var domainEntityTypeName = schemaObject.ParentElement.GetParentTypeName();
            var identityTypeName = schemaObject.XmlSchemaObjectName;
           
            if (identityTypeName.EndsWith(_expectedTypeSuffix))
                identityTypeName = identityTypeName.Substring(0, identityTypeName.Length - _expectedTypeSuffix.Length);
            return domainEntityTypeName == identityTypeName;
        }
    }
}