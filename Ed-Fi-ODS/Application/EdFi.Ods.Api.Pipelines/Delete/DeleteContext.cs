using System;
using EdFi.Ods.Common;

namespace EdFi.Ods.Pipelines.Delete
{
    public class DeleteContext : IHasIdentifier
    {
        public DeleteContext(Guid identifier)
        {
            Id = identifier;
        }

        public DeleteContext(Guid identifier, string etagValue)
        {
            Id = identifier;
            if(!string.IsNullOrWhiteSpace(etagValue)) ETag = etagValue;
        }

        public Guid Id { get; set; }
        public string ETag { get; set; }
    }
}