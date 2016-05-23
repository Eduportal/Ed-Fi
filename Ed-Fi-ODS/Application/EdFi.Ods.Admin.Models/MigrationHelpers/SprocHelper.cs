namespace EdFi.Ods.Admin.Models.MigrationHelpers
{
    public static class SprocHelper
    {
        public static string GenerateDropFor(string sprocName)
        {
            var sqlTemplate = @"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'P', N'PC'))" +
                              "DROP PROCEDURE [dbo].[{0}]";
            return string.Format(sqlTemplate, sprocName);
        }
    }
}