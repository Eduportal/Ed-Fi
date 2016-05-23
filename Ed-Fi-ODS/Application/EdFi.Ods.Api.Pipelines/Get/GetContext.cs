using System;
using EdFi.Ods.Common;
using EdFi.Ods.Entities.Common;

namespace EdFi.Ods.Pipelines.Get
{
    public class GetContext<TEntityModel> : IHasPersistentModel<TEntityModel>, IHasETag, IHasIdentifier
        where TEntityModel : class
    {
        public GetContext(Guid id, string etag)
        {
            Id = id;
            ETag = etag;
        }

        public string ETag { get; set; }
        public Guid Id { get; set; }
        public TEntityModel PersistentModel { get; set; }
    }
}