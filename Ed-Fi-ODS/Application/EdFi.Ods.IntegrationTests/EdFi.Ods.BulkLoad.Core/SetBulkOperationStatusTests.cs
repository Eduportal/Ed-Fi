using System;
using System.Linq;
using EdFi.Ods.BulkLoad.Core.Data;
using EdFi.Ods.IntegrationTests._Extensions;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Models.Resources.Enums;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.IntegrationTests.EdFi.Ods.BulkLoad.Core
{
    public class SetBulkOperationStatusTests
    {
        [TestFixture]
        public class When_operation_is_ready_and_we_set_status_to_started
        {
            private BulkOperation _result;
            private string _operationId;

            [TestFixtureSetUp]
            public void Setup()
            {
                var detractor = new BulkOperation {Id = Guid.NewGuid().ToString()};
                _operationId = Guid.NewGuid().ToString();
                var operation = new BulkOperation
                                    {
                                        Status = BulkOperationStatus.Ready,
                                        Id = _operationId,
                                    };

                var dbContext = new BulkOperationDbContext();
                dbContext.BulkOperations.Add(detractor);
                dbContext.BulkOperations.Add(operation);
                dbContext.SaveChangesForTest();

                var command = new SetBulkOperationStatus(() => new BulkOperationDbContext());
                command.SetStatus(_operationId, BulkOperationStatus.Started);

                using (var context = new BulkOperationDbContext())
                {
                    _result = context.BulkOperations.Single(x => x.Id == _operationId);
                }
            }

            [Test]
            public void Should_set_the_operation_state_to_started()
            {
                _result.Status.ShouldEqual(BulkOperationStatus.Started);
            }
        }
    }
}