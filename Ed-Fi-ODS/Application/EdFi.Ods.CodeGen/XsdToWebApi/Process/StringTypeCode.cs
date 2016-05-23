namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System;
    using System.Xml.Schema;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class StringTypeCode : ProcessChainOfResponsibilityBase
    {
        private readonly Type _expectedType = typeof(string);
        private readonly XmlTypeCode _xmlTypeCode;
        private readonly string _ruleName;

        public StringTypeCode(IProcessChainOfResponsibility next) : this(XmlTypeCode.String, next) {}

        protected StringTypeCode(XmlTypeCode xmlTypeCode, IProcessChainOfResponsibility next)
                : base(next)
        {
            this._xmlTypeCode = xmlTypeCode;
            this._ruleName = xmlTypeCode.ToString() + " Type Code";
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return !schemaObject.IsComplexType &&
                   schemaObject.XmlSchemaType.TypeCode == this._xmlTypeCode &&
                   !schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var parentRestType = this.GetContainingType(schemaObject);

            var objectName = schemaObject.XmlSchemaObjectName;

            var expectedRestProperty = new ExpectedTerminalRestProperty
                                           {
                                               PropertyName = this.AddPropertyContext(schemaObject, objectName),
                                                   PropertyExpectedRestType = new ExpectedRestType
                                                                                  {
                                                                                          ClassName = this._expectedType.Name,
                                                                                          Namespace = this._expectedType.Namespace
                                                                                  },
                                                   PropertyType = this._expectedType,
                                                   ContainingExpectedRestType = parentRestType,
                                                   IsNullable = this.GetIsOptional(schemaObject)
                                           };
            return new ProcessResult
                       {
                               ProcessChildren = false,
                               ProcessingRuleName = this._ruleName,
                               Expected = expectedRestProperty
                       };
        }
    }

    public class DurationTypeCode : StringTypeCode
    {
        public DurationTypeCode(IProcessChainOfResponsibility next) : base(XmlTypeCode.Duration, next) { }
    }

    public class TextTypeCode : StringTypeCode
    {
        public TextTypeCode(IProcessChainOfResponsibility next) : base(XmlTypeCode.Text, next) { }
    }
}