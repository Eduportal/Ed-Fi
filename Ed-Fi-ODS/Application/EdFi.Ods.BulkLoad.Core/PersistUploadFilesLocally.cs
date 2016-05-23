using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;

namespace EdFi.Ods.BulkLoad.Core
{
    public class PersistUploadFilesLocally : IPersistUploadFiles
    {
        private IStreamFileChunksToWriter writeFileChunksToDisk;
        private ICreateFilePathForUploadFile createFilePathForUploadFileStrategy;

        public PersistUploadFilesLocally(IStreamFileChunksToWriter writeFileChunksToDisk, ICreateFilePathForUploadFile createFilePathForUploadFile)
        {
            if (writeFileChunksToDisk == null)
                throw new ArgumentNullException("writeFileChunksToDisk");
            this.writeFileChunksToDisk = writeFileChunksToDisk;

            if (createFilePathForUploadFile == null)
                throw new ArgumentNullException("createFilePathForUploadFile");
            this.createFilePathForUploadFileStrategy = createFilePathForUploadFile;
        }

        public UploadFilePersistenceResult Persist(string operationId, string uploadFileId)
        {
            var filePathInfo = this.createFilePathForUploadFileStrategy.Create(operationId, uploadFileId);
            try
            {
                if (!Directory.Exists(filePathInfo.DirectoryPath))
                    Directory.CreateDirectory(filePathInfo.DirectoryPath);

                using (var fs = new FileStream(filePathInfo.FilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    this.writeFileChunksToDisk.Write(uploadFileId, fs);
                }

                return UploadFilePersistenceResult.WithSuccessfulFilePath(filePathInfo.FilePath);
            }
            catch (Exception e)
            {
                Trace.TraceError("Exception in {0}: {1}", GetType().Name, e);
                return UploadFilePersistenceResult.WithFailureMessage(e.Message);
            }
        }
    }

    public interface ICreateFilePathForUploadFile
    {
        CreateFilePathForUploadFileResult Create(string operationId, string uploadFileId);
    }

    public class CreateFilePathForUploadFileResult
    {
        public string FilePath { get; set; }
        public string DirectoryPath { get; set; }
    }

    public class CreateFilePathForUploadFileLocally : ICreateFilePathForUploadFile
    {
        public CreateFilePathForUploadFileResult Create(string operationId, string uploadFileId)
        {
            var folderPath = Path.Combine(Path.GetTempPath(), operationId);
            var fileName = uploadFileId + ".xml";
            var filePath = Path.Combine(folderPath, fileName);
            return new CreateFilePathForUploadFileResult() { DirectoryPath = folderPath, FilePath = filePath };
        }
    }
}