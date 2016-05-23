namespace EdFi.Ods.Common.Repositories
{
    public interface IDeleteEntityByKey<TEntity>
    {
        void DeleteByKey(TEntity specification, string etag);
    }
}