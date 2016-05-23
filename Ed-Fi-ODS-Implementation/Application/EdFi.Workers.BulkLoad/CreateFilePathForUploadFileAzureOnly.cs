using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EdFi.Ods.BulkLoad.Core;
using EdFi.Ods.Api.Data;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace EdFi.Workers.BulkLoad
{
    public class CreateFilePathForUploadFileAzureOnly : ICreateFilePathForUploadFile
    {
        private const string FileStorageNameKey = "UploadFileLocalStorageName";

        public CreateFilePathForUploadFileResult Create(string operationId, string uploadFileId)
        {
            var localStorageName = GetLocalStorageName();
            var storageConfig = RoleEnvironment.GetLocalResource(localStorageName);
            var directoryPath = Path.Combine(storageConfig.RootPath, operationId);
            var filePath = Path.Combine(storageConfig.RootPath, operationId, uploadFileId);
            return new CreateFilePathForUploadFileResult() { DirectoryPath = directoryPath, FilePath = filePath };
        }

        private static string GetLocalStorageName()
        {
            var localStorageName = CloudConfigurationManager.GetSetting(FileStorageNameKey);
            if (string.IsNullOrEmpty(localStorageName))
            {
                var message = string.Format("Configuration value for '{0}' is not set or empty", FileStorageNameKey);
                throw new Exception(message);
            }
            return localStorageName;
        }
    }
}