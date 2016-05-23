using System;
using EdFi.Ods.BulkLoad.Core.Data;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data;
using NUnit.Framework;
using Should;
using System.Linq;
using EdFi.Ods.IntegrationTests._Extensions;

namespace EdFi.Ods.IntegrationTests.EdFi.Ods.BulkLoad.Core
{
    public class FindBulkOperationExceptionsTests
    {
        [TestFixture]
        public class When_exceptions_are_stored_for_a_particular_file_and_we_search_for_exceptions_by_that_file
        {
            private BulkOperationException[] _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var detractorFile = new UploadFile {Id = Guid.NewGuid().ToString()};
                var detractorException = new BulkOperationException
                                             {
                                                 ParentUploadFile = detractorFile,
                                                 Message = "Don't find me",
                                                 DateTime = DateTime.Now
                                             };
                var uploadFile = new UploadFile {Id = Guid.NewGuid().ToString()};
                var exception1 = new BulkOperationException { ParentUploadFileId = uploadFile.Id, Message = "One", DateTime = DateTime.Now};
                var exception2 = new BulkOperationException { ParentUploadFileId = uploadFile.Id, Message = "Two", DateTime = DateTime.Now };
                using (var context = new BulkOperationDbContext())
                {
                    context.UploadFiles.Add(detractorFile);
                    context.UploadFiles.Add(uploadFile);
                    context.BulkOperationExceptions.Add(detractorException);
                    context.BulkOperationExceptions.Add(exception1);
                    context.BulkOperationExceptions.Add(exception2);
                    context.SaveChangesForTest();
                }

                var finder = new FindBulkOperationExceptions(() => new BulkOperationDbContext());
                _result = finder.FindByUploadFile(uploadFile.Id);
            }

            [Test]
            public void Should_find_only_the_exceptions_for_that_file()
            {
                _result.Length.ShouldEqual(2);
                _result.Count(x => x.Message == "One").ShouldEqual(1);
                _result.Count(x => x.Message == "Two").ShouldEqual(1);
            }
        }
    }
}