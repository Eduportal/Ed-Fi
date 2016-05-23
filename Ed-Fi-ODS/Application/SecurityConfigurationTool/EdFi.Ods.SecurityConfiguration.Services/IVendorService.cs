using System.Collections.Generic;
using EdFi.Ods.SecurityConfiguration.Services.Model;

namespace EdFi.Ods.SecurityConfiguration.Services
{
    public interface IVendorService
    {
        IEnumerable<Vendor> GetAll();
        Vendor GetById(int vendorId);
        int AddVendor(Vendor newVendor);
        void DeleteVendor(int vendorId);
        void UpdateVendor(Vendor vendor);
    }
}
