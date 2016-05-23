namespace EdFi.Ods.CodeGen.Models
{
    using EdFi.Ods.CodeGen.DatabaseSchema;

    public class ColumnModel : Column
    {
        public ColumnModel(Column col) : base(col)
        {
        }

        public bool IsLookup { get; set; }
        public string PropertyType { get; set; }
        public bool IsAutoIncrement { get; set; }
        public string ColNameToUse { get; set; }
        public bool IsUniqueId { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} ({1}.{2})", Name, TableSchema, TableName);
        }
    }
}