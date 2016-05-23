namespace EdFi.Ods.Api.Data.EventStore
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using EdFi.Ods.Api.Data;
    using EdFi.Ods.Api.Data.Contexts;
    using EdFi.Ods.Common;

    public class EventLogRepository : IEventLogRepository
    {
        private readonly IEnumerable<IObjectValidator> _objectValidators;
        private readonly IEventLogDbContext _context;

        public EventLogRepository(IEnumerable<IObjectValidator> objectValidators, IEventLogDbContext context)
        {
            this._objectValidators = objectValidators;
            this._context = context;
        }

        /// <summary>
        /// Add a row to the Azure table and return the row key
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public void Add(ApiEventLogEntry entry)
        {
            this.Validate(entry);
            this._context.EventLogEntries.Add(entry);
            this._context.SaveChanges();
        }

        private void Validate(ApiEventLogEntry entry)
        {
            var validatioResuts = this._objectValidators.ValidateObject(entry);

            if (validatioResuts.Any())
                throw new ValidationException(
                    string.Format("Validation failed: {0}",
                    validatioResuts
                    .Select(x => x.ErrorMessage)
                    .Aggregate((current, next) => current + next)));
        }

        public IQueryable<ApiEventLogEntry> GetAllEntriesByKey(string applicationKey)
        {
            return this._context.EventLogEntries.Where(x => x.ApplicationKey == applicationKey).AsQueryable();
        }

        public IQueryable<ApiEventLogEntry> GetEntries(string aggregateName, string aggregateKey, string applicationKey)
        {
             return this._context.EventLogEntries.Where(x => 
                x.ApplicationKey == applicationKey && 
                x.AggregateName == aggregateName &&
                x.AggregateKey == aggregateKey)
                .AsQueryable();
        }

    }
}
