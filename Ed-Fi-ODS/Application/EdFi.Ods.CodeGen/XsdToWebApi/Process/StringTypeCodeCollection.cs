using EdFi.Common.Inflection;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System;
    using System.Xml.Schema;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class StringTypeCodeCollection : ProcessChainOfResponsibilityBase
    {
        private static readonly Type _expectedType = typeof(string);
        private const XmlTypeCode _xmlTypeCode = XmlTypeCode.String;
        private const string _ruleName = "String Type Code Collection";

        public StringTypeCodeCollection(IProcessChainOfResponsibility next) : base(next) {}

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return !schemaObject.IsComplexType &&
                   schemaObject.XmlSchemaType.TypeCode == _xmlTypeCode &&
                   schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var parentTypeName = schemaObject.GetParentTypeName();
            var propertyTypeName = schemaObject.XmlSchemaType.Name;

            propertyTypeName = this.AddPropertyContext(schemaObject, propertyTypeName);
            if (!propertyTypeName.StartsWith(parentTypeName))
                propertyTypeName = parentTypeName + propertyTypeName;


            if (schemaObject.ParentElement.IsCommonSchemaTypeGroup())
            {
                var parentParentTypeName = schemaObject.ParentElement.GetParentTypeName();
                if (!propertyTypeName.StartsWith(parentParentTypeName))
                    propertyTypeName = parentParentTypeName + propertyTypeName;
            }

            var propertyName = CompositeTermInflector.MakePlural(propertyTypeName);
            var expectedRestType = new ExpectedRestType
                                       {
                                               ClassName = propertyTypeName,
                                               Namespace = this.GetNamespace(schemaObject)
                                       };
            var expectedRestProperty = new ExpectedInlineCollection
                                           {
                                                   ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                                   PropertyExpectedRestType = expectedRestType,
                                                   PropertyName = propertyName,
                                                   IsPropertyCollection = true,
                                                   TerminalRestProperty = new ExpectedTerminalRestProperty
                                                                           {
                                                                                  ContainingExpectedRestType = expectedRestType,
                                                                                  PropertyExpectedRestType = new ExpectedRestType
                                                                                                                 {
                                                                                                                    ClassName = _expectedType.Name,
                                                                                                                    Namespace = _expectedType.Namespace
                                                                                                                 },
                                                                                    PropertyName = schemaObject.XmlSchemaObjectName,
                                                                                    PropertyType = _expectedType,
                                                                                    IsNullable = false
                                                                           }
                                           };
            return new ProcessResult
                       {
                               ProcessChildren = false,
                               ProcessingRuleName = _ruleName,
                               Expected = expectedRestProperty
                       };
        }
    }
}