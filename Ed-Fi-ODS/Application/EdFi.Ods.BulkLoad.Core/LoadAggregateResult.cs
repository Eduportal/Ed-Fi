
namespace EdFi.Ods.BulkLoad.Core
{
    public class LoadAggregateResult
    {
        public LoadAggregateResult()
        {
            LoadExceptions = new LoadException[0];
        }

        public int RecordsLoaded { get; set; }
        public LoadException[] LoadExceptions { get; set; }
        public int SourceElementCount { get; set; }
    }
}