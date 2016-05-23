namespace EdFi.Ods.CodeGen.DatabaseSchema
{
    using System.Collections.Generic;

    public class Index
    {
        public Index()
        {
            this.ColumnNames = new List<string>();
        }

        public string IndexName { get; set; }
        public string TableName { get; set; }
        public List<string> ColumnNames { get; set; }
        public string SchemaName { get; set; }
    }
}