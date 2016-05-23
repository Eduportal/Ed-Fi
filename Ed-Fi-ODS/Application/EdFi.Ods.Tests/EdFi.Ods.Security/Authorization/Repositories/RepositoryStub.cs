namespace EdFi.Ods.Tests.EdFi.Ods.Security.Authorization.Repositories
{
    using System;

    using global::EdFi.Ods.Common;
    using global::EdFi.Ods.Common.Repositories;

    public class RepositoryStub<T> : IGetEntityById<T> 
        where T : IHasIdentifier, IDateVersionedEntity
    {
        private readonly T suppliedEntity;

        public RepositoryStub(T suppliedEntity)
        {
            this.suppliedEntity = suppliedEntity;
        }

        public bool GetByIdCalled { get; set; } 

        public T GetById(Guid id)
        {
            this.GetByIdCalled = true;

            return this.suppliedEntity;
        }
    }
}