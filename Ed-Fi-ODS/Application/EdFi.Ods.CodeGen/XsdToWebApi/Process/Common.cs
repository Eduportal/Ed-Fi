namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class Common : ProcessChainOfResponsibilityBase
    {
        public Common(IProcessChainOfResponsibility next)
            : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.IsCommonSchemaTypeGroup() &&
                   schemaObject.IsComplexType &&
                   !schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var parentTypeName = schemaObject.GetParentTypeName().StripExtensionNameValues();
            var schemaObjectTypeName = schemaObject.XmlSchemaType.Name.StripExtensionNameValues();

            var propertyTypeName = schemaObjectTypeName;
            if (!schemaObjectTypeName.StartsWith(parentTypeName))
                propertyTypeName = parentTypeName + schemaObjectTypeName;
            if (schemaObject.ParentElement.IsCommonSchemaTypeGroup())
            {
                var parentParentTypeName = schemaObject.ParentElement.GetParentTypeName();
                propertyTypeName = this.CombineTypeNames(parentParentTypeName, propertyTypeName);
            }

            var expectedRestProperty = new ExpectedRestProperty
                                           {
                                               ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                               PropertyExpectedRestType = new ExpectedRestType
                                                                              {
                                                                                  ClassName = propertyTypeName,
                                                                                  Namespace = this.GetNamespace(schemaObject)
                                                                              },
                                               PropertyName = propertyTypeName
                                           };

            return new ProcessResult
                       {
                           ProcessChildren = true,
                           ProcessingRuleName = "Common",
                           Expected = expectedRestProperty
                       };
        }
    }
}