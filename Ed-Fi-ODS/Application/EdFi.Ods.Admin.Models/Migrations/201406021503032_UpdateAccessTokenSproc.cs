using System.Data.Entity.Migrations;
using EdFi.Ods.Admin.Models.MigrationHelpers;
using EdFi.Ods.Admin.Models.Sql;
using EdFi.Ods.Common.Utils.Resources;

namespace EdFi.Ods.Admin.Models
{
    public partial class UpdateAccessTokenSproc : DbMigration
    {
        public override void Up()
        {
            Sql(SprocHelper.GenerateDropFor("AccessTokenIsValid"));
            Sql(EmbeddedResourceReader.GetResourceString<AdminModelsSqlMarker>("Sproc_AdminTokenIsValid_2014_06_02.sql"));
        }

        public override void Down()
        {
            Sql(SprocHelper.GenerateDropFor("AccessTokenIsValid"));
            Sql(EmbeddedResourceReader.GetResourceString<AdminModelsSqlMarker>("Sproc_AdminTokenIsValid_2014_05_08.sql"));
        }
    }
}