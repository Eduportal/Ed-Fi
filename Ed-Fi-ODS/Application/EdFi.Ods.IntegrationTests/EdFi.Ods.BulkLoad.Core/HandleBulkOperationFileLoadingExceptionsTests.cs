using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EdFi.Ods.BulkLoad.Core;
using EdFi.Ods.BulkLoad.Core.Data;
using EdFi.Ods.Common;
using EdFi.Ods.Common.ExceptionHandling;
using EdFi.Ods.Common.ExceptionHandling.Translators;
using EdFi.Ods.IntegrationTests._Extensions;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data;
using log4net;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.IntegrationTests.EdFi.Ods.BulkLoad.Core
{
    using global::EdFi.Ods.Tests._Bases;

    public class HandleBulkOperationFileLoadingExceptionsTests
    {
        [TestFixture]
        public class When_handling_file_loading_exceptions : TestBase
        {
            private BulkOperationException[] _results;
            private DateTime _now = new DateTime(2010, 3, 4);
            private string _fileId;

            [TestFixtureSetUp]
            public void Setup()
            {
                InitSystemClock(_now);
                _fileId = Guid.NewGuid().ToString();
                var operation = new BulkOperation {Id = Guid.NewGuid().ToString(), UploadFiles = new List<UploadFile> {new UploadFile {Id = _fileId}}};
                using (var context = new BulkOperationDbContext())
                {
                    context.BulkOperations.Add(operation);
                    context.SaveChangesForTest();
                }
                var errorProvider = new RESTErrorProvider(new[]{new BadRequestExceptionTranslator(), });
                var persister = new PersistBulkOperationExceptions(() => new BulkOperationDbContext(), errorProvider);
                var exceptions = new[] { new LoadException { Element = "FooEl", Exception = new Exception("Foo") }, new LoadException { Element = "BarEl", Exception = new Exception("Bar") } };
                persister.HandleFileLoadingExceptions(_fileId, exceptions);
                using (var context = new BulkOperationDbContext())
                {
                    _results = context.BulkOperationExceptions.Where(x => x.ParentUploadFileId == _fileId).ToArray();
                }
            }

            [Test]
            public void Should_create_a_bulk_exception_for_each_message()
            {
                _results.Length.ShouldEqual(2);
            }

            [Test]
            public void Should_set_upload_file_id()
            {
                _results.All(x => x.ParentUploadFileId == _fileId).ShouldBeTrue();
            }

            [Test]
            public void Should_set_exception_message()
            {
                _results.Any(x => x.Message.Contains("An unexpected error")).ShouldBeTrue();
            }

            [Test]
            public void Should_set_exception_code()
            {
                _results.All(x => x.Code == 500).ShouldBeTrue();
            }

            [Test]
            public void Should_set_time()
            {
                _results.All(x => x.DateTime.ToShortDateString().Equals(_now.ToShortDateString()));
            }
        }
    }
}