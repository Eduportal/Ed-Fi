using System.Collections.Generic;

namespace EdFi.Ods.Common.Repositories
{
    public interface IGetEntitiesBySpecification<TEntity>
        where TEntity : IHasIdentifier, IDateVersionedEntity
    {
        IList<TEntity> GetBySpecification(TEntity specification, IQueryParameters queryParameters);
    }
}