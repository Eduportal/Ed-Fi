namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System;

    using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class Association : ProcessChainOfResponsibilityBase
    {
        public Association(IProcessChainOfResponsibility next) : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaTypeGroup == "Association" &&
                   schemaObject.IsComplexType &&
                   !schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var parent = schemaObject.ParentElement as ParsedInterchange;
            if (parent == null)
                throw new Exception("Association not at root " + schemaObject.XmlSchemaObjectName);

            var expectedRestProperty = new ExpectedRestType
                                            {
                                                ClassName = schemaObject.XmlSchemaType.Name.StripExtensionNameValues(),
                                                Namespace = this.GetAggregateNamespace(schemaObject)
                                            };

            return new ProcessResult
                        {
                            ProcessChildren = true,
                            ProcessingRuleName = "Association",
                            Expected = expectedRestProperty
                        };
        }
    }
}