using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdFi.Common.SchoolIdentity
{
    public interface IUniqueSchoolIdentity
    {
        ISchoolIdentity Get(string uniqueId);
        ISchoolIdentity[] Get(ISchoolIdentity identity);
        ISchoolIdentity Post(ISchoolIdentity command);
    }
}
