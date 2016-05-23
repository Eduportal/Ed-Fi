namespace EdFi.Ods.Common.Repositories
{
    public interface ICreateEntity<in TEntity> 
        where TEntity : IHasIdentifier, IDateVersionedEntity
    {
        void Create(TEntity entity, bool enforceOptimisticLock);
    }
}