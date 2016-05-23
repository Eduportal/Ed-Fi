namespace EdFi.Ods.Admin.UITests
{
    using System.IO;

    public static class DirectoryInfoExtensions
    {
        public static void DeleteFiles(this DirectoryInfo directory, params string[] searchPatterns)
        {
            foreach (string searchPattern in searchPatterns)
                foreach (var file in directory.GetFiles(searchPattern, SearchOption.AllDirectories)) 
                    file.Delete();
        }
    }
}
