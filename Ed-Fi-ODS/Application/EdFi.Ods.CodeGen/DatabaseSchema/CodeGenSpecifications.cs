using System.Linq;

namespace EdFi.Ods.CodeGen.DatabaseSchema
{
    public static class CodeGenSpecifications
    {
        private static string[] UsiColumns
        {
            get
            {
                return new[]
                {
                    "StaffUSI",
                    "StudentUSI",
                    "ParentUSI"
                };
            }
        }

        public static bool IsUsiColumn(string columnName)
        {
            return UsiColumns.Contains(columnName);
        }
    }
}