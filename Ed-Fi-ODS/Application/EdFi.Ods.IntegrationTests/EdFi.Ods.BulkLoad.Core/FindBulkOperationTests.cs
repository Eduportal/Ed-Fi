using System;
using System.Collections.Generic;
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
    using global::EdFi.Ods.Tests._Bases;

    public class FindBulkOperationTests
    {
        [TestFixture]
        public class When_operation_is_ready_with_upload_files_and_we_start
        {
            private BulkOperation _result;
            private BulkOperation _databaseContents;
            private string _operationId;
            private string _fileId1;
            private string _fileId2;

            [TestFixtureSetUp]
            public void Setup()
            {
                var detractor = new BulkOperation {Id = Guid.NewGuid().ToString()};

                _fileId1 = Guid.NewGuid().ToString();
                _fileId2 = Guid.NewGuid().ToString();
                _operationId = Guid.NewGuid().ToString();
                var operation = new BulkOperation
                                    {
                                        Status = BulkOperationStatus.Ready,
                                        Id = _operationId,
                                        UploadFiles = new List<UploadFile> {new UploadFile {Id = _fileId1}, new UploadFile {Id = _fileId2}}
                                    };

                var dbContext = new BulkOperationDbContext();
                dbContext.BulkOperations.Add(detractor);
                dbContext.BulkOperations.Add(operation);
                dbContext.SaveChangesForTest();

                var command = new FindBulkOperations(() => new BulkOperationDbContext());
                _result = command.FindAndStart(_operationId);

                using (var context = new BulkOperationDbContext())
                {
                    _databaseContents = context.BulkOperations.Single(x => x.Id == _operationId);
                }
            }

            [Test]
            public void Should_set_the_operation_state_to_started()
            {
                _databaseContents.Status.ShouldEqual(BulkOperationStatus.Started);
            }

            [Test]
            public void Should_find_the_correct_operation()
            {
                _result.Id.ShouldEqual(_operationId);
            }

            [Test]
            public void Should_include_upload_files()
            {
                _result.UploadFiles.Count.ShouldEqual(2);
                _result.UploadFiles.Select(x => x.Id).ShouldContain(_fileId1);
                _result.UploadFiles.Select(x => x.Id).ShouldContain(_fileId2);
            }
        }

        [TestFixture]
        public class When_operation_exists_with_upload_files
        {
            private BulkOperation _result;
            private string _operationId;
            private string _fileId1;
            private string _fileId2;

            [TestFixtureSetUp]
            public void Setup()
            {
                var detractor = new BulkOperation {Id = Guid.NewGuid().ToString()};

                _fileId1 = Guid.NewGuid().ToString();
                _fileId2 = Guid.NewGuid().ToString();
                _operationId = Guid.NewGuid().ToString();
                var operation = new BulkOperation
                                    {
                                        Status = BulkOperationStatus.Ready,
                                        Id = _operationId,
                                        UploadFiles = new List<UploadFile> {new UploadFile {Id = _fileId1}, new UploadFile {Id = _fileId2}}
                                    };

                var dbContext = new BulkOperationDbContext();
                dbContext.BulkOperations.Add(detractor);
                dbContext.BulkOperations.Add(operation);
                dbContext.SaveChangesForTest();

                var command = new FindBulkOperations(() => new BulkOperationDbContext());
                _result = command.FindWithFiles(_operationId);
            }

            [Test]
            public void Should_find_the_correct_operation()
            {
                _result.Id.ShouldEqual(_operationId);
            }

            [Test]
            public void Should_include_upload_files()
            {
                _result.UploadFiles.Count.ShouldEqual(2);
                _result.UploadFiles.Select(x => x.Id).ShouldContain(_fileId1);
                _result.UploadFiles.Select(x => x.Id).ShouldContain(_fileId2);
            }
        }

        [TestFixture]
        public class When_operation_exists_with_upload_files_and_we_find_without_files : TestBase
        {
            private BulkOperation _result;
            private string _operationId;
            private string _fileId1;
            private string _fileId2;

            [TestFixtureSetUp]
            public void Setup()
            {
                var detractor = new BulkOperation {Id = Guid.NewGuid().ToString()};

                _fileId1 = Guid.NewGuid().ToString();
                _fileId2 = Guid.NewGuid().ToString();
                _operationId = Guid.NewGuid().ToString();
                var operation = new BulkOperation
                                    {
                                        Status = BulkOperationStatus.Ready,
                                        Id = _operationId,
                                        UploadFiles = new List<UploadFile> {new UploadFile {Id = _fileId1}, new UploadFile {Id = _fileId2}}
                                    };

                var dbContext = new BulkOperationDbContext();
                dbContext.BulkOperations.Add(detractor);
                dbContext.BulkOperations.Add(operation);
                dbContext.SaveChangesForTest();

                var command = new FindBulkOperations(() => new BulkOperationDbContext());
                _result = command.FindWithoutFiles(_operationId);
            }

            [Test]
            public void Should_find_the_correct_operation()
            {
                _result.Id.ShouldEqual(_operationId);
            }

            [Test]
            public void Should_not_include_upload_files()
            {
                TestForException<ObjectDisposedException>(() => _result.UploadFiles.ToArray());
            }
        }
    }
}