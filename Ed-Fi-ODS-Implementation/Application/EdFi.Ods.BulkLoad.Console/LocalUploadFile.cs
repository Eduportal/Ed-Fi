namespace EdFi.Ods.BulkLoad.Console
{
    public class LocalUploadFile
    {
        public LocalUploadFile(string id, string filePath)
        {
            Id = id;
            FilePath = filePath;
        }

        public string Id { get; private set; }
        public string FilePath { get; private set; }
    }
}