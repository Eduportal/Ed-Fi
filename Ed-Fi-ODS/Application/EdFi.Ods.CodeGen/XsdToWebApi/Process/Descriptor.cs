namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class Descriptor : ProcessChainOfResponsibilityBase
    {
        public Descriptor(IProcessChainOfResponsibility next) : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaTypeGroup == "Descriptor" &&
                   !schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var parent = schemaObject.ParentElement as ParsedInterchange;
            if (parent == null)
                throw new Exception("Descriptor not at root " + schemaObject.XmlSchemaObjectName);

            var expectedType = new ExpectedRestType
                                    {
                                        ClassName = schemaObject.XmlSchemaType.Name,
                                        Namespace = this.GetAggregateNamespace(schemaObject)
                                    };

            return new ProcessResult
                       {
                               ProcessChildren = true,
                               ProcessingRuleName = "Descriptor",
                               Expected = expectedType
                       };
        }
    }
}