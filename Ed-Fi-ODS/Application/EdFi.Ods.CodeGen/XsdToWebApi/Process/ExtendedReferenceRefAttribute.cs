namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System.Xml.Schema;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class ExtendedReferenceRefAttribute : ProcessChainOfResponsibilityBase
    {
        private const string _referenceTypeSuffix = "ReferenceType";

        public ExtendedReferenceRefAttribute(IProcessChainOfResponsibility next) : base(next)
        {
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.XmlSchemaObjectName == "ref" &&
                   schemaObject.IsAttribute &&
                   schemaObject.XmlSchemaType.TypeCode == XmlTypeCode.Idref &&
                   (schemaObject.ParentElement.XmlSchemaTypeGroup == "Extended Reference" ||
                   schemaObject.ParentElement.XmlSchemaTypeGroup == "Extended Descriptor Reference");
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var referencedElementName = schemaObject.ParentXmlSchemaType.Name;

            if (referencedElementName.EndsWith(_referenceTypeSuffix))
                referencedElementName = referencedElementName.Substring(0, referencedElementName.Length - _referenceTypeSuffix.Length);

            return new ProcessResult
                       {
                               ProcessChildren = false,
                               ProcessingRuleName = "Extended Reference Ref Attribute",
                               Expected = new ExpectedReferencedElement
                                              {
                                                      ReferencedElementName = referencedElementName
                                              }
                       };
        }
    }
}