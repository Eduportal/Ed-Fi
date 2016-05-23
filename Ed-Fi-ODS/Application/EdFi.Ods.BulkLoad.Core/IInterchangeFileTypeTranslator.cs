namespace EdFi.Ods.BulkLoad.Core
{
    public interface IInterchangeFileTypeTranslator
    {
        string GetInterchangeType(string fileName);
    }
}