namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System;

    using EdFi.Common.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using EdFi.Ods.CodeGen.XsdToWebApi.Process;

    public class InlineEntityProperty : IInlineEntityProperty
    {
        private ParsedSchemaObject _property;
        private ExpectedInlineCollection _target;

        public InlineEntityProperty(ParsedSchemaObject property)
        {
            if(!property.IsInlineProperty()) 
                throw new ArgumentException(string.Format("{0} is a {1} object and not an ExpectedInlineCollection type.", 
                    property.XmlSchemaObjectName, property.ProcessResult.Expected.GetType()));
            this._property = property;
            this._target = property.ProcessResult.Expected as ExpectedInlineCollection;
        }

        public string ClassName { get { return this._target.PropertyExpectedRestType.GetClassName(); } }
        public string PropertyName { get { return this._target.PropertyName; }}
        public string ElementName { get { return this._property.XmlSchemaObjectName; } }
        public string InlinePropertyName { get { return  this._target.TerminalRestProperty.PropertyName; } }
        public Type InlinePropertyType { get { return this._target.TerminalRestProperty.PropertyType; } }
        public string InterfaceName { get { return "I" + this._target.PropertyExpectedRestType.GetClassName(); } }
        public string Namespace { get { return this._target.PropertyExpectedRestType.GetNamespace(); } }
        public bool IsSchoolYear { get { return this._property.ProcessResult.ProcessingRuleName == "School Year"; } }
        public bool IsNested { get { return this._property.ParentElement.IsCommonExpansion(); } }

        public string[] ElementNames
        {
            get
            {
                return !this.IsNested ? new[] {this.ElementName} : new[] {this._property.ParentElement.XmlSchemaObjectName, this.ElementName};
            }
        }
    }

    public static class InlineEntityPropertyExtensions
    {
        public static string GetConvertExpression(this IInlineEntityProperty property, string xElementVariableName)
        {
            if (!property.InlinePropertyType.IsValueType && property.InlinePropertyType != typeof(string))
                throw new NotSupportedException(string.Format("Type of inline property {1} must be either value type or string. Type {0} is not supported.", property.InlinePropertyType.Name, property.PropertyName));

            if (property.IsSchoolYear)
            {
                return string.Format(property.InlinePropertyType.IsNullable() ? 
                            @"{0}.Last4CharactersAsNullableShort()" : 
                            @"{0}.Last4CharactersAsNullableShort() == null ? (Int16){0}.Last4CharactersAsNullableShort() : default(Int16)", 
                            xElementVariableName);
            }

            return string.Format("{0}.ValueOrDefault<{1}{2}>()",
                xElementVariableName, Nullable.GetUnderlyingType(property.InlinePropertyType) ?? property.InlinePropertyType,
                property.InlinePropertyType.IsNullable() ? "?" : string.Empty);
        }
    }
}