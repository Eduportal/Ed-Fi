using EdFi.Ods.Admin.Models.MigrationHelpers;
using EdFi.Ods.Admin.Models.Sql;
using EdFi.Ods.Common.Utils.Resources;

namespace EdFi.Ods.Admin.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReturnClaimSetNameFromAdminTokenIsValidSproc : DbMigration
    {
        public override void Up()
        {
            Sql(SprocHelper.GenerateDropFor("AccessTokenIsValid"));
            Sql(EmbeddedResourceReader.GetResourceString<AdminModelsSqlMarker>("Sproc_AdminTokenIsValid_201412191503090.sql"));
        }

        public override void Down()
        {
            Sql(SprocHelper.GenerateDropFor("AccessTokenIsValid"));
            Sql(EmbeddedResourceReader.GetResourceString<AdminModelsSqlMarker>("Sproc_AdminTokenIsValid_201406022004546.sql"));
        }
    }
}
