using System.Data.Entity;
using EdFi.Ods.Api.Data;

namespace EdFi.Ods.Api.Data.Contexts
{
    public interface IEventLogDbContext : IDbContext
    {
        IDbSet<ApiEventLogEntry> EventLogEntries { get; set; }
    }
}