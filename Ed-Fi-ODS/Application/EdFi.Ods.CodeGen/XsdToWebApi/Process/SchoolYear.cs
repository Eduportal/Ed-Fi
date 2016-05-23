namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class SchoolYear : ProcessChainOfResponsibilityBase
    {
        private static readonly Type _expectedType = typeof(short);
        private static readonly Type _nullableExpectedType = typeof(short?);

        private const string _expectedSchemaType = "SchoolYearType";

        public SchoolYear(IProcessChainOfResponsibility next) : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaType.Name == _expectedSchemaType &&
                   schemaObject.XmlSchemaTypeGroup == "Enumeration" &&
                   !schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var objectName = schemaObject.XmlSchemaObjectName;

            var isOptional = this.GetIsOptional(schemaObject);

            var expectedRestProperty = new ExpectedTerminalRestProperty
                                           {
                                               ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                               PropertyExpectedRestType = new ExpectedRestType
                                                                               {
                                                                                   ClassName = isOptional ? _expectedType.Name + "?" : _expectedType.Name,
                                                                                   Namespace = _expectedType.Namespace
                                                                               },
                                               PropertyType = isOptional ? _nullableExpectedType : _expectedType,
                                               PropertyName = this.AddPropertyContext(schemaObject, objectName),
                                               IsNullable = isOptional
                                           };

            return new ProcessResult
                       {
                               ProcessChildren = false,
                               ProcessingRuleName = "School Year",
                               Expected = expectedRestProperty
                       };
        }
    }
}