namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class ExtendedDescriptorReference : ProcessChainOfResponsibilityBase
    {
        private const string _expectedTypeSuffix = "DescriptorReferenceType";
        private const string _expectedRestPropertySuffix = "Descriptor";

        public ExtendedDescriptorReference(IProcessChainOfResponsibility next)
                : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaTypeGroup == "Extended Descriptor Reference" &&
                   !schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var containingType = this.GetContainingType(schemaObject);


            var schemaObjectTypeName = schemaObject.XmlSchemaType.Name.StripProjectPrefix();
            var baseDescriptorName = schemaObjectTypeName;
            if (baseDescriptorName.EndsWith(_expectedTypeSuffix))
                baseDescriptorName = baseDescriptorName.Substring(0, baseDescriptorName.Length - _expectedTypeSuffix.Length);

            var schemaObjectName = schemaObject.XmlSchemaObjectName;
            var context = string.Empty;
            if (schemaObjectName.EndsWith(baseDescriptorName))
                context = schemaObjectName.Substring(0, schemaObjectName.Length - baseDescriptorName.Length);

            var parentContext = schemaObject.ParentElement.ProcessResult.Expected as ExpectedContext;
            if (parentContext != null && !context.StartsWith(parentContext.Context))
                context = parentContext.Context + context;
            if (baseDescriptorName.StartsWith(context))
                context = string.Empty;

            var expectedContext = new ExpectedContext(containingType)
                                            {
                                                Context = context + baseDescriptorName + _expectedRestPropertySuffix,
                                                ContextNullable = schemaObject.IsOptional
                                            };

            return new ReferenceTypeProcessResult
                        {
                            ProcessChildren = true,
                            ProcessingRuleName = "Extended Descriptor Reference",
                            ReferenceTypeName = schemaObjectTypeName,
                            Expected = expectedContext
                        };
        }
    }
}