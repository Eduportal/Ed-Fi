using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EdFi.Common.IO;
using EdFi.Ods.Common;

namespace EdFi.Ods.BulkLoad.Console
{
    public class ValidateAndSourceLocalOnlyFiles : IValidateAndSourceFiles
    {
        private readonly IEnumerable<LocalUploadFile> _localUploadFiles;
        private readonly IConfigurationAccess _configurationAccess;
        private readonly IFileSystem _fileSystem;
        private readonly string _workingFolder;

        public ValidateAndSourceLocalOnlyFiles(IEnumerable<LocalUploadFile> localUploadFiles,
                                               IConfigurationAccess configurationAccess, IFileSystem fileSystem)
        {
            _localUploadFiles = localUploadFiles;
            _configurationAccess = configurationAccess;
            _fileSystem = fileSystem;
            _workingFolder = _configurationAccess.BulkOperationWorkingFolder ?? _fileSystem.GetTempPath();
            if (string.IsNullOrWhiteSpace(_workingFolder))
                throw new ArgumentException("WorkingFolder must be configured");
        }

        public IUploadFileSourcingResults ValidateMakeLocalAndFindPath(string operationId, string uploadFileId)
        {
            var localFilePath = _localUploadFiles.Single(x => x.Id == uploadFileId).FilePath;
            string workingFilePath = Path.Combine(_workingFolder, uploadFileId);
            _fileSystem.CopyFile(localFilePath, workingFilePath);
            return UploadFileSourcingResults.WithSuccessPath(workingFilePath);
        }
    }
}