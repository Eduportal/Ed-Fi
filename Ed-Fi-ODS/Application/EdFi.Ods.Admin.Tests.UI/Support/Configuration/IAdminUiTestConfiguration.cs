namespace EdFi.Ods.Admin.UITests.Support.Configuration
{
    public interface IAdminUiTestConfiguration
    {
        string DbConnectionStringName { get; }
    }

    public class AdminUiTestConfiguration : IAdminUiTestConfiguration
    {
        public string DbConnectionStringName
        {
            get { return "EdFi_Admin"; }
        }
    }
}