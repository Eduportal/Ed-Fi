namespace EdFi.Ods.Utilities.LoadGeneration.Infrastructure
{
    public class FileWrapper : IFile
    {
        public string ReadAllText(string path)
        {
            return System.IO.File.ReadAllText(path);
        }
    }
}