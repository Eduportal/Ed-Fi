namespace EdFi.Ods.BulkLoad.Core
{
    public interface IDirectoryLocator
    {
        string GetNamedDirectory(string directoryName);
    }
}