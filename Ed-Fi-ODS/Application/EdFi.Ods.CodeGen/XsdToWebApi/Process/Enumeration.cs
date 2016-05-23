namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System;

    using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class Enumeration : ProcessChainOfResponsibilityBase
    {
        private static readonly Type _expectedType = typeof(string);
        private const string _expectedPropertySuffix = "Type";

        public Enumeration(IProcessChainOfResponsibility next) : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaTypeGroup == "Enumeration" &&
                   !schemaObject.IsComplexType &&
                   !schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var propertyName = schemaObject.XmlSchemaObjectName.StripExtensionNameValues();
            
            if (!propertyName.EndsWith(_expectedPropertySuffix))
                propertyName = propertyName + _expectedPropertySuffix;

            var expectedRestProperty = new ExpectedTerminalRestProperty
                                            {
                                                ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                                PropertyExpectedRestType = new ExpectedRestType
                                                {
                                                    ClassName = _expectedType.Name,
                                                    Namespace = _expectedType.Namespace
                                                },
                                                PropertyName = this.AddPropertyContext(schemaObject, propertyName),
                                                PropertyType = _expectedType,
                                                IsNullable = this.GetIsOptional(schemaObject)
                                           };
            return new ProcessResult
                       {
                               ProcessChildren = false,
                               ProcessingRuleName = "Enumeration",
                               Expected = expectedRestProperty
                       };
        }
    }
}