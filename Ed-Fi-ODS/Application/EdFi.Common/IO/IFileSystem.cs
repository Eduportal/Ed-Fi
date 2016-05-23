namespace EdFi.Common.IO
{
    public interface IFileSystem
    {
        string[] GetFilesInDirectory(string path);
        string GetParentDirectory(string path);
        string GetCurrentDirectory();
        string GetAssemblyLocation<T>();
        bool DirectoryExists(string path);
        string GetTempPath();
        void CopyFile(string sourcePath, string destinationPath);
        string GetFilenameFromPath(string path);
    }
}