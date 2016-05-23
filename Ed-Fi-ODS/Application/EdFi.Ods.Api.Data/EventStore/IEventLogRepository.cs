namespace EdFi.Ods.Api.Data.EventStore
{
    using System.Linq;

    using EdFi.Ods.Api.Data;

    public interface IEventLogRepository
    {
        void Add(ApiEventLogEntry entry);
        IQueryable<ApiEventLogEntry> GetEntries(string aggregateName, string aggregateKey, string applicationKey);
        IQueryable<ApiEventLogEntry> GetAllEntriesByKey(string applicationKey);
    }
}
