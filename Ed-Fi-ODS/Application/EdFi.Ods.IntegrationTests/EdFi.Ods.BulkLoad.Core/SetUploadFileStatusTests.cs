using System;
using EdFi.Ods.BulkLoad.Core.Data;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Models.Resources.Enums;
using NUnit.Framework;
using System.Linq;
using Should;

namespace EdFi.Ods.IntegrationTests.EdFi.Ods.BulkLoad.Core
{
    public class SetUploadFileStatusTests
    {
        [TestFixture]
        public class When_status_is_incomplete_and_we_set_to_ready
        {
            private UploadFile _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var fileId = Guid.NewGuid().ToString();
                var file = new UploadFile {Id = fileId};
                using(var context = new BulkOperationDbContext())
                {
                    context.UploadFiles.Add(file);
                    context.SaveChanges();
                }

                var setter = new SetUploadFileStatus(() => new BulkOperationDbContext());
                setter.SetStatus(fileId, UploadFileStatus.Ready);

                using (var context = new BulkOperationDbContext())
                {
                    _result = context.UploadFiles.Single(x => x.Id == fileId);
                }
            }

            [Test]
            public void Should_set_file_status()
            {
                _result.Status.ShouldEqual(UploadFileStatus.Ready);
            }
        }
    }
}