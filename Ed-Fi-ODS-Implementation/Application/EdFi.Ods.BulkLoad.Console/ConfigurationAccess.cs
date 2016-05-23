using System.Configuration;

namespace EdFi.Ods.BulkLoad.Console
{
    public class ConfigurationAccess : IConfigurationAccess
    {
        public string BaseOdsConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["EdFi_Ods"].ConnectionString; }
        }

        public string BulkOperationWorkingFolder
        {
            get { return ConfigurationManager.AppSettings["BulkOperationWorkingFolder"]; }
        }
    }
}