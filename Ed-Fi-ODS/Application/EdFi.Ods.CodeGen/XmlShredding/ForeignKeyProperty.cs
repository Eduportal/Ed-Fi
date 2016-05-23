namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System;

    using EdFi.Common.Extensions;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class ForeignKeyProperty : SimpleTypeProperty, IForeignKeyProperty
    {
        private readonly string _map;

        public ForeignKeyProperty(ParsedSchemaObject property, string map) : base(property)
        {
            this._map = map;
        }

        public string SerializedReferenceMap { get { return this._map; }}

        public new bool IsSchoolYear
        {
            get
            {
                return this._property.ProcessResult != null && this._property.ProcessResult.ProcessingRuleName == "School Year";
            }
        }
    }

    public static class ForeignKeyPropertyExtensions
    {
        public static string GetConvertExpression(this IForeignKeyProperty property, string xElementVariableName)
        {
            if (!property.PropertyType.IsValueType && property.PropertyType != typeof(string))
                throw new NotSupportedException(string.Format("Type of foreign key property {0} must be either value type or string. Type {1} is not supported.", property.PropertyName, property.PropertyType.Name));

            if (property.IsSchoolYear)
                return string.Format(@"{0}.Last4CharactersAsNullableShort() == null ? default(Int16) : (Int16){0}.Last4CharactersAsNullableShort()", xElementVariableName);

            return string.Format("{0}.ValueOrDefault<{1}{2}>()",
                xElementVariableName, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType,
                property.PropertyType.IsNullable() ? "?" : string.Empty);
        }
    }
}