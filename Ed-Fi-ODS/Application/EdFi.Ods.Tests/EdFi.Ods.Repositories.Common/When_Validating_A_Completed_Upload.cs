namespace EdFi.Ods.Tests.EdFi.Ods.Repositories.Common
{
    using System;

    using global::EdFi.Ods.Api.Data;
    using global::EdFi.Ods.Api.Data.Contexts;
    using global::EdFi.Ods.Api.Data.Repositories.BulkOperations;
    using global::EdFi.Ods.Common.Context;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    [TestFixture]
    public class When_Validating_A_Completed_Upload : TestBase
    {
        [Test]
        public void And_the_File_Has_Expired_Or_Completed_Processing()
        {
            var uploadId = Guid.NewGuid().ToString();
            var mockDb = this.Stub<IDbExecutor<IBulkOperationDbContext>>();
            var mockSchoolYearProvider = this.Stub<ISchoolYearContextProvider>();
            mockSchoolYearProvider.Stub(x => x.GetSchoolYear()).Return(DateTime.Now.Year);
            var action = default(Func<IBulkOperationDbContext, UploadFile>);
            mockDb.Stub(x => x.Get(action)).IgnoreArguments().Return(new UploadFile());
            var sut = new UploadValidator(mockDb, mockSchoolYearProvider);
            var errors = sut.IsValid(uploadId);
            errors.Any.ShouldBeTrue();
        }

        [Test]
        public void And_the_File_Exists()
        {
            var uploadId = Guid.NewGuid().ToString();
            var mockDb = this.Stub<IDbExecutor<IBulkOperationDbContext>>();
            var mockSchoolYearProvider = this.Stub<ISchoolYearContextProvider>();
            var action = default(Func<IBulkOperationDbContext, UploadFile>);
            mockDb.Stub(x => x.Get(action)).IgnoreArguments().Return(new UploadFile{Id = uploadId.ToString()});
            mockDb.Stub(x => x.Get(default(Func<IBulkOperationDbContext, BulkOperation>)))
                .IgnoreArguments()
                .Return(new BulkOperation { SchoolYear = DateTime.Now.Year });
            mockSchoolYearProvider.Stub(x => x.GetSchoolYear()).Return(DateTime.Now.Year);
            var sut = new UploadValidator(mockDb, mockSchoolYearProvider);
            var errors = sut.IsValid(uploadId);
            errors.Any.ShouldBeFalse();           
        }
    }
}