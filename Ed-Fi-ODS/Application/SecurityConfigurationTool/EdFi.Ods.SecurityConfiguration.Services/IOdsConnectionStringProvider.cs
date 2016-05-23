using System.Collections.Generic;
using System.Configuration;

namespace EdFi.Ods.SecurityConfiguration.Services
{
    public interface IOdsConnectionStringProvider
    {
        IEnumerable<ConnectionStringSettings> GetOdsConnectionStrings();
    }
}