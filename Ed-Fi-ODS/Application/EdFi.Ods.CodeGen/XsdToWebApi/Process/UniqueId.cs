namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class UniqueId : ProcessChainOfResponsibilityBase
    {
        private const string _expectedPropertySuffix = "UniqueId";
        private readonly Type _expectedType = typeof(string);

        private const string _expectedSchemaType = "UniqueId";

        public UniqueId(IProcessChainOfResponsibility next)
            : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaObjectName.EndsWith(_expectedPropertySuffix) &&
                   schemaObject.XmlSchemaType.Name == _expectedSchemaType &&
                   schemaObject.XmlSchemaTypeGroup == "Simple" &&
                   !schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var expectedRestProperty = new ExpectedTerminalRestProperty
                                            {
                                                ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                                PropertyExpectedRestType = new ExpectedRestType
                                                {
                                                    ClassName = this._expectedType.Name,
                                                    Namespace = this._expectedType.Namespace
                                                },
                                                PropertyType = this._expectedType,
                                                PropertyName = schemaObject.XmlSchemaObjectName,
                                                IsNullable = this.GetIsOptional(schemaObject)
                                            };

            return new ProcessResult
                        {
                            ProcessChildren = false,
                            ProcessingRuleName = "Unique Id",
                            Expected = expectedRestProperty
                        };
        }
    }
}