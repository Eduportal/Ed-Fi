using EdFi.Common.Inflection;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System.Linq;
    using System.Text;

    using EdFi.Common.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class CommonCollection : ProcessChainOfResponsibilityBase
    {
        public CommonCollection(IProcessChainOfResponsibility next)
                : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.IsCommonSchemaTypeGroup() &&
                   schemaObject.IsComplexType &&
                   schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var parentTypeName = schemaObject.GetParentTypeName();
            var schemaObjectTypeName = schemaObject.XmlSchemaObjectName;// schemaObject.XmlSchemaType.Name;

            var propertyTypeName = this.CombineTypeNames(parentTypeName, schemaObjectTypeName);
            if (schemaObject.ParentElement.IsCommonSchemaTypeGroup())
            {
                var parentParentTypeName = schemaObject.ParentElement.GetParentTypeName();
                propertyTypeName = this.CombineTypeNames(parentParentTypeName, propertyTypeName);
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
                               ProcessingRuleName = "Common Collection",
                               Expected = expectedRestProperty
                       };
        }
    }
}