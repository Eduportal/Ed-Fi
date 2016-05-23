using EdFi.Ods.Admin.Models.MigrationHelpers;
using EdFi.Ods.Admin.Models.Sql;
using EdFi.Ods.Common.Utils.Resources;

namespace EdFi.Ods.Admin.Models
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNamespacePrefixToVendor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Vendors", "NamespacePrefix", c => c.String(maxLength: 255));

            Sql(SprocHelper.GenerateDropFor("AccessTokenIsValid"));
            Sql(EmbeddedResourceReader.GetResourceString<AdminModelsSqlMarker>("Sproc_AdminTokenIsValid_201501052052517.sql"));
        }
        
        public override void Down()
        {
            Sql(SprocHelper.GenerateDropFor("AccessTokenIsValid"));
            Sql(EmbeddedResourceReader.GetResourceString<AdminModelsSqlMarker>("Sproc_AdminTokenIsValid_201412191503090.sql"));

            DropColumn("dbo.Vendors", "NamespacePrefix");
        }
    }
}
