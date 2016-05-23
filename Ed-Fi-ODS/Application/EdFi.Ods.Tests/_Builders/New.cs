namespace EdFi.Ods.Tests._Builders
{
    using global::EdFi.Ods.Common;
    using global::EdFi.Ods.Tests.EdFi.Ods.Common._Stubs.Repositories;

    public static class New
    {
        public static StubRepositoryBuilder<TEntity> StubRepository<TEntity>()
            where TEntity : IHasIdentifier, IDateVersionedEntity
        {
            return new StubRepositoryBuilder<TEntity>();
        }
    }
}