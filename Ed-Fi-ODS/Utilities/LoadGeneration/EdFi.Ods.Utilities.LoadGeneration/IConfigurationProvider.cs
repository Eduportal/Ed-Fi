namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface IConfigurationProvider
    {
        string AdminUrl { get; }
        string ApiUrl { get; }
        string ApiKey { get; }
        string ApiSecret { get; }
    }
}