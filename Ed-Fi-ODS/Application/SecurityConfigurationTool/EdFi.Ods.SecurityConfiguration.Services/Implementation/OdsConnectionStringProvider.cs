using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace EdFi.Ods.SecurityConfiguration.Services.Implementation
{
    public class OdsConnectionStringProvider : IOdsConnectionStringProvider
    {
        public IEnumerable<ConnectionStringSettings> GetOdsConnectionStrings()
        {
            return ConfigurationManager.ConnectionStrings
               .Cast<ConnectionStringSettings>()
               .Where(c => c.Name.StartsWith("EdFi_ODS_", StringComparison.CurrentCultureIgnoreCase));
        }
    }
}