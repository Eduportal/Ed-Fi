using System;

namespace EdFi.Ods.Common.Repositories
{
    public interface IDeleteEntityById<TEntity>
    {
        void DeleteById(Guid id, string etag);
    }
}