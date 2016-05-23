using System.Collections.Generic;
using EdFi.Ods.Api.Models.Resources.Enums;

namespace EdFi.Ods.Api.Data
{
    public class BulkOperation
    {
        public BulkOperation()
        {
            UploadFiles = new List<UploadFile>();
        }

        public string Id { get; set; }
        public virtual ICollection<UploadFile> UploadFiles { get; set; }
        public string ResetDistrictData { get; set; }
        public BulkOperationStatus Status { get; set; }
        public string DatabaseName { get; set; }
        public int SchoolYear { get; set; }
    }
}
