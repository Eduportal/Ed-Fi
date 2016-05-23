namespace EdFi.Ods.Common.Repositories
{
    public interface IUpsertEntity<TEntity> 
        where TEntity : IHasIdentifier, IDateVersionedEntity
    {
        TEntity Upsert(TEntity entity, bool enforceOptimisticLock, out bool isModified, out bool isCreated);
    }
}