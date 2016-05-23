using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Common.IO;
using EdFi.Ods.BulkLoad.Core;
using EdFi.Ods.BulkLoad.Core.Data;
using EdFi.Ods.Common.Utils.Extensions;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Models.Resources.Enums;

namespace EdFi.Ods.BulkLoad.Console
{
    public class LocalBulkOperationInitializer
    {
        private readonly IInterchangeFileTypeTranslator _translator;
        private readonly ICreateBulkOperation _createBulkOperation;
        private readonly BulkLoaderConfiguration _config;
        private readonly IFileSystem _fileSystem;

        public LocalBulkOperationInitializer(IInterchangeFileTypeTranslator translator,
                                             ICreateBulkOperation createBulkOperation,
                                             BulkLoaderConfiguration config,
                                             IFileSystem fileSystem)
        {
            _translator = translator;
            _createBulkOperation = createBulkOperation;
            _config = config;
            _fileSystem = fileSystem;
        }


        public LocalBulkOperation CreateOperationAndGetLocalFiles()
        {
            var sourceFiles = GetFilesInSourceFolder(_config.SourceFolder).ToArray();
            if (sourceFiles.None())
                return LocalBulkOperation.Empty;

            var operation = new BulkOperation {Id = Guid.NewGuid().ToString(), DatabaseName = _config.DatabaseNameOverride};
            var localUploadFiles = new List<LocalUploadFile>();
            foreach (var file in sourceFiles)
            {
                var uploadFile = new UploadFile
                                     {
                                         Id = Guid.NewGuid().ToString(),
                                         Format = "text/xml",
                                         Status = UploadFileStatus.Ready,
                                         InterchangeType = file.InterchangeType,
                                     };

                operation.UploadFiles.Add(uploadFile);
                localUploadFiles.Add(new LocalUploadFile(uploadFile.Id, file.FilePath));
            }
            _createBulkOperation.Create(operation);

            return new LocalBulkOperation(operation.Id, localUploadFiles);
        }

        private class InterchangeFile
        {
            public string FilePath { get; set; }
            public string InterchangeType { get; set; }
        }

        private IEnumerable<InterchangeFile> GetFilesInSourceFolder(string sourceFolder)
        {
            if (!_fileSystem.DirectoryExists(sourceFolder))
                throw new ArgumentException(string.Format("Source path '{0}' not found", sourceFolder));

            var files = _fileSystem.GetFilesInDirectory(sourceFolder);

            return (from s in files
                    let fileName = _fileSystem.GetFilenameFromPath(s)
                    where fileName != null
                    let interchangeType = _translator.GetInterchangeType(fileName)
                    where !string.IsNullOrWhiteSpace(interchangeType)
                    select new InterchangeFile
                               {
                                   FilePath = s,
                                   InterchangeType = interchangeType
                               }).ToList();
        }
    }
}