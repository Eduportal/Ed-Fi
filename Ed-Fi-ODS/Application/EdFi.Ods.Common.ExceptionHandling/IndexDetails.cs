using System.Collections.Generic;

namespace EdFi.Ods.Common.ExceptionHandling
{
    public class IndexDetails
    {
        public string IndexName { get; set; }
        public string TableName { get; set; }
        public List<string> ColumnNames { get; set; }
    }
}