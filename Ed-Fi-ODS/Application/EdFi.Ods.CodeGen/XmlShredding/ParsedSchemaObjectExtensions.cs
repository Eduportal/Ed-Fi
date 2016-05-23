namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System.Collections.Generic;
    using System.Linq;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using EdFi.Ods.CodeGen.XsdToWebApi.Process;

    public static class ParsedSchemaObjectExtensions
    {
        public static string EXTENDEDREFERENCETYPEGROUP = "Extended Reference";

        public static bool IsEntityProperty(this ParsedSchemaObject p)
        {
            return p.IsInlineProperty() || p.IsRestProperty();
        }

        public static bool IsSingleEntity(this ParsedSchemaObject p)
        {
            return !p.IsCollection
                    && p.IsRestProperty()
                    && !p.IsDescriptorsTypeEnumeration();
        }

        public static bool IsRestType(this ParsedSchemaObject p)
        {
            return p.IsNotNull() && p.ProcessResult.Expected is ExpectedRestType;
        }

        public static bool IsExtendedReference(this ParsedSchemaObject p)
        {
            return p.IsCollection == false 
                && p.XmlSchemaTypeGroup == EXTENDEDREFERENCETYPEGROUP 
                && p.ProcessResult != null 
                && !(p.ProcessResult.Expected is Skip);
        }

        public static bool IsExtendedReferenceCollection(this ParsedSchemaObject p)
        {
            return p.IsCollection
                   && p.XmlSchemaTypeGroup == EXTENDEDREFERENCETYPEGROUP
                   && p.ProcessResult != null
                   && !(p.ProcessResult.Expected is Skip);
        }

        public static bool IsDescriptorsExtRef(this ParsedSchemaObject p)
        {
            return p.XmlSchemaTypeGroup == "Extended Descriptor Reference";
        }

        public static bool ContainsForeignKey(this ParsedSchemaObject p)
        {
            return p.IsExtendedReference() || p.IsIdentity() || p.IsDescriptorsExtRef();
        }

        public static bool IsIdentity(this ParsedSchemaObject p)
        {
            return p.XmlSchemaTypeGroup == "Identity";
        }

        public static bool IsReference(this ParsedSchemaObject p)
        {
            return p.IsNotNull() && p.ProcessResult.Expected is ExpectedReferencedElement;
        }

        public static bool IsInlineProperty(this ParsedSchemaObject p)
        {
            return p.IsNotNull() && p.ProcessResult.Expected is ExpectedInlineCollection;
        }

        public static bool IsNotNull(this ParsedSchemaObject p)
        {
            return p.ProcessResult != null && p.ProcessResult.Expected != null;
        }

        public static bool IsTerminalProperty(this ParsedSchemaObject p)
        {
            return p.IsNotNull() && p.ProcessResult.Expected is ExpectedTerminalRestProperty;
        }

        public static bool IsRestProperty(this ParsedSchemaObject p)
        {
            return p.IsNotNull() && p.ProcessResult.Expected is ExpectedRestProperty && !p.IsInlineProperty() && !p.IsTerminalProperty();
        }

        public static bool IsDescriptorsTypeEnumeration(this ParsedSchemaObject p)
        {
            return p.IsNotNull() && (p.IsRestProperty() && p.ProcessResult.ProcessingRuleName == "Descriptor Enumeration");
        }

        public static bool IsCommonExpansion(this ParsedSchemaObject p)
        {
            return p.IsNotNull() && p.ProcessResult.ProcessingRuleName == "Common Expansion" && p.ChildElements.Any();
        }

        public static bool IsEdOrgReference(this ParsedSchemaObject p)
        {
            return p.XmlSchemaObjectName.Equals("EducationOrganizationReference");
        }

        public static bool IsEntityCollection(this ParsedSchemaObject p)
        {
            return p.IsNotNull() && p.IsCollection
                   && (p.XmlSchemaTypeGroup == EXTENDEDREFERENCETYPEGROUP || p.IsDescriptorsExtRef()
                       || (p.IsRestProperty() && !p.IsDescriptorsTypeEnumeration()));
        }

        public static bool ShouldBeSkipped(this ParsedSchemaObject p)
        {
            if (p.ProcessResult == null || p.ProcessResult.Expected == null) return true;
            return p.ProcessResult.Expected is Skip;
        }

        public static IEnumerable<ParsedSchemaObject> GetChildElementsToBeParsed(this ParsedSchemaObject p)
        {
            return p.ChildElements.Where(c => !c.ShouldBeSkipped()).ToArray();
        }
    }
}