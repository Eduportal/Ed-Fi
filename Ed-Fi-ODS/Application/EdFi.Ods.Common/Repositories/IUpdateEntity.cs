namespace EdFi.Ods.Common.Repositories
{
    public interface IUpdateEntity<TEntity> 
        where TEntity : IHasIdentifier, IDateVersionedEntity
    {
        void Update(TEntity persistentEntity);
    }
}