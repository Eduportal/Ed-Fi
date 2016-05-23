namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class IdentityResource : ProcessChainOfResponsibilityBase
    {
        private const string _expectedTypeSuffix = "Identity";

        public IdentityResource(IProcessChainOfResponsibility next)
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

            var collectionContainingType = schemaObject.ParentElement.ProcessResult.Expected as ExpectedMultipleRestProperties;
            if (collectionContainingType != null)
                containingType = collectionContainingType.AdditionalProperty.PropertyExpectedRestType;

            var context = containingType as ExpectedContext;
            if (context == null)
            {
                context = new ExpectedContext(containingType)
                              {
                                      ContextNullable = false,
                                      Context = string.Empty
                              };
            }

            return new ProcessResult
                       {
                           ProcessChildren = schemaObject.ParentElement.ProcessResult.Expected != null,
                           ProcessingRuleName = "Identity Resource",
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