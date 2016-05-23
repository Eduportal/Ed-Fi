using EdFi.Common.Inflection;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class ExtendedReferenceCollection : ProcessChainOfResponsibilityBase
    {
        private const string _expectedTypeSuffix = "Reference";

        public ExtendedReferenceCollection(IProcessChainOfResponsibility next)
                : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaTypeGroup == "Extended Reference" &&
                   schemaObject.IsCollection &&
                   schemaObject.XmlSchemaObjectName.EndsWith(_expectedTypeSuffix);
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var parentTypeName = schemaObject.GetParentTypeName();

            var propertyTypeName = schemaObject.XmlSchemaObjectName;
            if (propertyTypeName.EndsWith(_expectedTypeSuffix))
                propertyTypeName = propertyTypeName.Substring(0, propertyTypeName.Length - _expectedTypeSuffix.Length);
            
            propertyTypeName = this.AddPropertyContext(schemaObject, propertyTypeName);
            propertyTypeName = parentTypeName + propertyTypeName;

            if (schemaObject.ParentElement.IsCommonSchemaTypeGroup())
            {
                var parentParentTypeName = schemaObject.ParentElement.GetParentTypeName();
                if (!propertyTypeName.StartsWith(parentParentTypeName))
                    propertyTypeName = parentParentTypeName + propertyTypeName;
            }

            var propertyName = CompositeTermInflector.MakePlural(propertyTypeName);
            var expectedRestProperty = new ExpectedRestProperty
                                            {
                                                ContainingExpectedRestType = this.GetContainingType(schemaObject),
                                                PropertyExpectedRestType = new ExpectedRestType
                                                                    {
                                                                        ClassName = propertyTypeName,
                                                                        Namespace = this.GetNamespace(schemaObject)
                                                                    },
                                                PropertyName = propertyName,
                                                IsPropertyCollection = true
                                            };

            return new ProcessResult
                       {
                               ProcessChildren = true,
                               ProcessingRuleName = "Extended Reference Collection",
                               Expected = expectedRestProperty
                       };
        }
    }
}