namespace EdFi.Ods.SecurityConfiguration.Services.Model
{
    public class Vendor
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public VendorContact MainContact { get; set; }
        public string NamespacePrefix { get; set; }
        public int ApplicationCount { get; set; }
        public int SandboxesCount { get; set; }

        /* Note: 
         * although the underlying architecture supports multi-contact for a vendor, 
         * the security configuration tools uses a single contact for each vendor at the moment.
        
         * public IEnumerable<VendorContact> Contacts { get; set; }
         
         */
    }
}