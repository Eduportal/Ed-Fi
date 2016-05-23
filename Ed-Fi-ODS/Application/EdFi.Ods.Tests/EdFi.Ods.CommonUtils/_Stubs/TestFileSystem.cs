namespace EdFi.Ods.Tests.EdFi.Ods.CommonUtils._Stubs
{
    using System;
    using System.Collections.Generic;

    using global::EdFi.Common.IO;

    public class TestFileSystem : IFileSystem
    {
        private string _tempPath;
        private readonly List<CopiedFile> _copiedFiles = new List<CopiedFile>();

        public string[] GetFilesInDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public string GetParentDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public string GetCurrentDirectory()
        {
            throw new NotImplementedException();
        }

        public string GetAssemblyLocation<T>()
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public string GetTempPath()
        {
            if (this._tempPath == null)
                throw new Exception("Temporary path not initialized");
            return this._tempPath;
        }

        public class CopiedFile
        {
            public string SourcePath { get; set; }
            public string DestinationPath { get; set; }
        }


        public void CopyFile(string sourcePath, string destinationPath)
        {
            this._copiedFiles.Add(new CopiedFile {SourcePath = sourcePath, DestinationPath = destinationPath});
        }

        public string GetFilenameFromPath(string path)
        {
            throw new NotImplementedException();
        }

        public CopiedFile[] CopiedFiles
        {
            get { return this._copiedFiles.ToArray(); }
        }

        //////////////////////////////
        /// Builder helpers
        //////////////////////////////
        public TestFileSystem WithTempPath(string path)
        {
            this._tempPath = path;
            return this;
        }
    }
}