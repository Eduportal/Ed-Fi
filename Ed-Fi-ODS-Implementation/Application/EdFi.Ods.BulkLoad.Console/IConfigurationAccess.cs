namespace EdFi.Ods.BulkLoad.Console
{
    public interface IConfigurationAccess
    {
        string BaseOdsConnectionString { get; }
        string BulkOperationWorkingFolder { get; }
    }
}