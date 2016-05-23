using EdFi.Ods.Common.ExceptionHandling.Translators;

namespace EdFi.Ods.Common.ExceptionHandling
{
    public interface IDatabaseMetadataProvider
    {
        IndexDetails GetIndexDetails(string indexName);
    }
}