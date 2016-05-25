using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Common.SchoolIdentity;
using EdFi.Common.StaffIdentity;

namespace EdFi.Ods.Api.Data.Repositories.StaffIdentifier
{
    public class UniqueStaffIdentity : IUniqueStaffIdentity
    {
        public IStaffIdentity Get(string uniqueId)
        {
            return new StaffIdentity
            {
                EducationOrganizationId = 12345
            };
        }

        public IStaffIdentity[] Get(IStaffIdentity identity)
        {
            return new IStaffIdentity[] { identity };
        }
    }
}
