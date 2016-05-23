namespace EdFi.Ods.Tests.EdFi.Ods.WebApi.Data
{
    using System.Data.Entity;

    using global::EdFi.Ods.Api.Data;
    using global::EdFi.Ods.Api.Data.Contexts;

    using NUnit.Framework;

    using Rhino.Mocks;

    public class DbExecutorTests
    {
        [TestFixture]
        public class When_applying_changes_with_new_entity
        {
            [Test]
            public void Should_persist_new_entity()
            {
                var dbContext = MockRepository.GenerateMock<IBulkOperationDbContext>();
                var mockSet = MockRepository.GenerateStub<IDbSet<BulkOperation>>();
                dbContext.Stub(x => x.BulkOperations).Return(mockSet);

                new DbExecutor<IBulkOperationDbContext>(() => dbContext).ApplyChanges(ctx => ctx.BulkOperations.Add(new BulkOperation()));

                dbContext.AssertWasCalled(x => x.BulkOperations.Add(Arg<BulkOperation>.Is.Anything));
                dbContext.AssertWasCalled(x => x.SaveChanges());
            }
        }
    }
}
