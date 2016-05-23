using EdFi.Common.Inflection;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class ExtendedDescriptorReferenceCollection : ProcessChainOfResponsibilityBase
    {
        private const string _expectedTypeSuffix = "DescriptorReferenceType";
        private const string _annotationName = "Descriptor";

        public ExtendedDescriptorReferenceCollection(IProcessChainOfResponsibility next)
                : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaTypeGroup == "Extended Descriptor Reference" &&
                   schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var schemaObjectTypeName = schemaObject.XmlSchemaType.Name.StripProjectPrefix();
            var baseDescriptorName = schemaObjectTypeName;
            if (baseDescriptorName.EndsWith(_expectedTypeSuffix))
                baseDescriptorName = baseDescriptorName.Substring(0, baseDescriptorName.Length - _expectedTypeSuffix.Length);

            var schemaObjectName = schemaObject.XmlSchemaObjectName;
            var context = string.Empty;
            if (schemaObjectName.EndsWith(baseDescriptorName))
                context = schemaObjectName.Substring(0, schemaObjectName.Length - baseDescriptorName.Length);

            var propertyTypeName = context + baseDescriptorName;


            var parentTypeName = schemaObject.GetParentTypeName();
            if (!propertyTypeName.StartsWith(parentTypeName))
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

            return new ReferenceTypeProcessResult
                        {
                            ProcessChildren = true,
                            ProcessingRuleName = "Extended Descriptor Reference Collection",
                            ReferenceTypeName = schemaObjectTypeName,
                            Expected = expectedRestProperty
                        };
        }
    }
}