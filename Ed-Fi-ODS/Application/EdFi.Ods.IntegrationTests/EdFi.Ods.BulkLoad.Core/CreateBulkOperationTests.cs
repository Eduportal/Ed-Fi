using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    public class CreateBulkOperationTests
    {
        [TestFixture]
        public class When_bulk_operation_is_created_with_upload_files
        {
            private string _operationId = Guid.NewGuid().ToString();
            private string _fileId1 = Guid.NewGuid().ToString();
            private string _fileId2 = Guid.NewGuid().ToString();
            private BulkOperation _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var operation = new BulkOperation
                                    {
                                        Status = BulkOperationStatus.Ready,
                                        Id = _operationId,
                                        UploadFiles =
                                            new List<UploadFile>
                                                {
                                                    new UploadFile {Id = _fileId1},
                                                    new UploadFile {Id = _fileId2}
                                                }
                                    };

                var creator = new CreateBulkOperation(() => new BulkOperationDbContext());
                DbContextExtensions.CatchAndWriteDbValidationExceptions(() => creator.Create(operation));
                using (var context = new BulkOperationDbContext())
                {
                    _result =
                        context.BulkOperations.AsQueryable()
                               .Include(x => x.UploadFiles)
                               .SingleOrDefault(x => x.Id == _operationId);
                }
            }

            [Test]
            public void Should_create_bulk_operation_and_upload_files()
            {
                _result.ShouldNotBeNull();
                _result.Id.ShouldEqual(_operationId);
                _result.UploadFiles.Count.ShouldEqual(2);
                _result.UploadFiles.Count(x => x.Id == _fileId1).ShouldEqual(1);
                _result.UploadFiles.Count(x => x.Id == _fileId2).ShouldEqual(1);
            }
        }
    }
}