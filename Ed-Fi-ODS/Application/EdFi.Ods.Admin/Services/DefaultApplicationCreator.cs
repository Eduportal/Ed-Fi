using EdFi.Ods.Admin.Models;
using System.Linq;
using System.Data.Entity;

namespace EdFi.Ods.Admin.Services
{
    public class DefaultApplicationCreator : IDefaultApplicationCreator
    {
        private readonly UsersContext _context;
        private readonly IDatabaseTemplateLeaQuery _leaQuery;

        public DefaultApplicationCreator(UsersContext context, IDatabaseTemplateLeaQuery leaQuery)
        {
            _context = context;
            _leaQuery = leaQuery;
        }

        /// <summary>
        /// Look for an existing default application for this particular sandbox type.  Also, make sure that all
        /// Local Education Agency associations are updated.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="sandboxType"></param>
        /// <returns></returns>
        public Application FindOrCreateUpdatedDefaultSandboxApplication(int vendorId, SandboxType sandboxType)
        {
            var vendor = _context.Vendors
                                 .Where(x => x.VendorId == vendorId)
                                 .Include(x => x.Applications.Select(a => a.ApplicationEducationOrganizations))
                                 .Single();

            var leaIds = _leaQuery.GetLocalEducationAgencyIds(sandboxType);
            var applicationName = string.Format("Default Sandbox Application {0}", sandboxType);
            var application = GetApplication(vendor, applicationName);
            
            foreach (var leaId in leaIds)
            {
                if (application.ApplicationEducationOrganizations.Any(x => x.EducationOrganizationId == leaId))
                    continue;
                application.CreateEducationOrganizationAssociation(leaId);
            }

            _context.SaveChanges();
            return application;
        }

        private Application GetApplication(Vendor vendor, string applicationName)
        {
            if (vendor.Applications.Any(x => x.ApplicationName == applicationName))
                return vendor.Applications.Single(x => x.ApplicationName == applicationName);

            var newApplication = vendor.CreateApplication(applicationName);
            _context.Applications.Add(newApplication);
            return newApplication;
        }
    }
}