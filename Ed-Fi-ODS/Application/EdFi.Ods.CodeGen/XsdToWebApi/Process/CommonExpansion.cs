namespace EdFi.Ods.CodeGen.XsdToWebApi.Process
{
    using System.Collections.Generic;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class CommonExpansion : ProcessChainOfResponsibilityBase
    {
        // TODO: load this from xml metadata
        private static readonly List<string> _explosionTypes = new List<string>
                                                   {
                                                           "Credits",
                                                           "MeetingTime",
                                                           "Name",
                                                           "Citizenship",
                                                           "EmploymentPeriod",
                                                           "Achievement",
                                                           "BirthData",
                                                           "AttendanceEvent",
                                                           "EducationContentSource",
                                                   };

        public CommonExpansion(IProcessChainOfResponsibility next)
                : base(next)
        {
        }

        public static bool IsCommonExpansionSchemaType(string xmlSchemaTypeName)
        {
            return _explosionTypes.Contains(xmlSchemaTypeName);
        }

        protected override bool CanProcess(ParsedSchemaObject schemaObject)
        {
            return schemaObject.IsCommonSchemaTypeGroup() &&
                   IsCommonExpansionSchemaType(schemaObject.XmlSchemaType.Name) &&
                   schemaObject.IsComplexType &&
                   !schemaObject.IsCollection;
        }

        protected override ProcessResult DoProcessSchemaObject(ParsedSchemaObject schemaObject)
        {
            var parentType = this.GetContainingType(schemaObject);

            var context = string.Empty;

            var schemaObjectName = schemaObject.XmlSchemaObjectName;
            var schemaObjectTypeName = schemaObject.XmlSchemaType.Name;
            if (schemaObjectName.EndsWith(schemaObjectTypeName))
                context = schemaObjectName.Substring(0, schemaObjectName.Length - schemaObjectTypeName.Length);

            var expectedRestProperty = new ExpectedContext
                                           {
                                               ClassName = parentType.GetClassName(),
                                               Namespace = parentType.GetNamespace(),
                                               Context = context,
                                               ContextNullable = schemaObject.IsOptional
                                           };

            return new ProcessResult
                       {
                               ProcessChildren = true,
                               ProcessingRuleName = "Common Expansion",
                               Expected = expectedRestProperty
                       };
        }
    }
}