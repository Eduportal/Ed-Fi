using System.IO;

namespace EdFi.Common.IO
{
    public class FileSystemWrapper : IFileSystem
    {
        public string[] GetFilesInDirectory(string path)
        {
            return Directory.GetFiles(path);
        }

        public string GetParentDirectory(string path)
        {
            var directoryInfo = Directory.GetParent(path);
            if (directoryInfo == null)
                return null;
            return directoryInfo.FullName;
        }

        public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public string GetAssemblyLocation<T>()
        {
            return Path.GetDirectoryName(typeof (T).Assembly.Location);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public string GetTempPath()
        {
            return Path.GetTempPath();
        }

        public void CopyFile(string sourcePath, string destinationPath)
        {
            File.Copy(sourcePath, destinationPath);
        }

        public string GetFilenameFromPath(string path)
        {
            return Path.GetFileName(path);
        }
    }
}