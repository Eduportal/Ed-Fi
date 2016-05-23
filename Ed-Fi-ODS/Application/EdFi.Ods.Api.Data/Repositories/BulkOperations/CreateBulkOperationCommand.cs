using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Api.Models.Resources;

namespace EdFi.Ods.Api.Data.Repositories.BulkOperations
{
    public class CreateBulkOperationCommand
    {
        private IEnumerable<string> _validationErrors;

        public IEnumerable<string> ValidationErrors
        {
            get { return _validationErrors; }
            set
            {
                _validationErrors = value;
                Invalid = _validationErrors != null && _validationErrors.Any();
            }
        }

        public bool Invalid { get; private set; }
        public string ResetDistrictData { get; set; }
        public BulkOperationResource Resource { get; set; }
        public IEnumerable<UploadFileRequest> UploadFiles { get; set; }
        public string ApiKey { get; set; }
        public int SchoolYear { get; set; }
    }
}