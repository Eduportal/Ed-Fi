using System.Collections.Generic;
using EdFi.Ods.SecurityConfiguration.Services.Model;

namespace EdFi.Ods.SecurityConfiguration.Services
{
    public interface IApplicationService
    {
        IEnumerable<Application> GetVendorApplications(int vendorId);
        Application GetById(int vendorId, int applicationId);
        int AddApplication(int vendorId, Application newApplication);
        void DeleteApplication(int vendorId, int applicationId);
        void UpdateApplication(int vendorId, Application updatingApplication);
        KeyGenResult GenerateApplicationKey(int vendorId, int applicationId);
    }
}