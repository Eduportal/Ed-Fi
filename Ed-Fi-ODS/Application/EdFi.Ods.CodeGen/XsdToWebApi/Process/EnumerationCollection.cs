using EdFi.Common.Inflection;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System;
    using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class EnumerationCollection : ProcessChainOfResponsibilityBase
    {
        private static readonly Type _expectedType = typeof(string);
        private const string _expectedTypeSuffix = "Type";

        public EnumerationCollection(IProcessChainOfResponsibility next)
            : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaTypeGroup == "Enumeration" &&
                   !schemaObject.IsComplexType &&
                   schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var schemaObjectTypeName = schemaObject.XmlSchemaType.Name.StripExtensionNameValues();
            var baseEnumerationName = schemaObjectTypeName;
            if (baseEnumerationName.EndsWith(_expectedTypeSuffix))
                baseEnumerationName = baseEnumerationName.Substring(0, baseEnumerationName.Length - _expectedTypeSuffix.Length);

            var schemaObjectName = schemaObject.XmlSchemaObjectName;
            var context = string.Empty;
            if (schemaObjectName.EndsWith(baseEnumerationName))
                context = schemaObjectName.Substring(0, schemaObjectName.Length - baseEnumerationName.Length);

            var propertyTypeName = context + baseEnumerationName;

            var parentTypeName = schemaObject.DeclaringSchemaType.Name.StripExtensionNameValues();
            if (schemaObject.ParentElement.IsCommonSchemaTypeGroup() && CommonExpansion.IsCommonExpansionSchemaType(parentTypeName))
                parentTypeName = schemaObject.ParentElement.DeclaringSchemaType.Name.StripExtensionNameValues();
            if (!propertyTypeName.StartsWith(parentTypeName))
                propertyTypeName = parentTypeName + propertyTypeName;

            if (schemaObject.ParentElement.IsCommonSchemaTypeGroup())
            {
                var parentParentTypeName = schemaObject.ParentElement.DeclaringSchemaType.Name.StripExtensionNameValues();
                if (!propertyTypeName.StartsWith(parentParentTypeName))
                    propertyTypeName = parentParentTypeName + propertyTypeName;
            }

            var propertyName = CompositeTermInflector.MakePlural(propertyTypeName);
            var expectedRestType = new ExpectedRestType
                                        {
                                            ClassName = propertyTypeName,
                                            Namespace = this.GetNamespace(schemaObject)
                                        };

            var terminalPropertyName = schemaObject.XmlSchemaObjectName;
            if (terminalPropertyName.StartsWith(context))
                terminalPropertyName = terminalPropertyName.Substring(context.Length, terminalPropertyName.Length - context.Length);
            if (!terminalPropertyName.EndsWith(_expectedTypeSuffix))
                terminalPropertyName = terminalPropertyName + _expectedTypeSuffix;

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
                                                    PropertyName = terminalPropertyName,
                                                    PropertyType = _expectedType,
                                                    IsNullable = false
                                                }
                                            };
            return new ProcessResult
                        {
                            ProcessChildren = false,
                            ProcessingRuleName = "Enumeration Collection",
                            Expected = expectedRestProperty
                        };
        }
    }
}