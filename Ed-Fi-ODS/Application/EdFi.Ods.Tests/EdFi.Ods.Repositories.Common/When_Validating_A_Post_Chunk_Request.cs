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
    public class When_validating_a_post_chunk_with_non_matching_school_year : TestBase
    {
        [Test]
        public void Should_return_year_mismatch_error()
        {
            var uploadId = Guid.NewGuid().ToString();
            var mockDb = this.Stub<IDbExecutor<IBulkOperationDbContext>>();
            var mockSchoolYearProvider = this.Stub<ISchoolYearContextProvider>();
            mockSchoolYearProvider.Stub(x => x.GetSchoolYear()).Return(DateTime.Now.Year);
            var action = default(Func<IBulkOperationDbContext, UploadFile>);
            mockDb.Stub(x => x.Get(action)).IgnoreArguments().Return(new UploadFile { Id = uploadId });
            mockDb.Stub(x => x.Get(default(Func<IBulkOperationDbContext, BulkOperation>)))
                .IgnoreArguments()
                .Return(new BulkOperation {SchoolYear = DateTime.Now.Year + 1});
            var sut = new UploadValidator(mockDb, mockSchoolYearProvider);
            var validationErrors = sut.IsValid(uploadId);
            validationErrors.Any.ShouldBeTrue();
            validationErrors.ErrorMessage.ShouldEqual(
                string.Format("School year of {0} does not match bulk operation's school year of {1}.",
                    DateTime.Now.Year, DateTime.Now.Year + 1));
        }
    }

    [TestFixture]
    public class When_Validating_A_Chunk_Post_Request : TestBase
    {
        [Test]
        public void And_The_Upload_Exists_And_The_Offset_Plus_Size_Is_Less_Than_Total_File_Size_Should_Return_Valid()
        {
            var uploadId = Guid.NewGuid().ToString();
            var offset = 0;
            var size = 10000000;
            var mockDb = this.Stub<IDbExecutor<IBulkOperationDbContext>>();
            var mockSchoolYearProvider = this.Stub<ISchoolYearContextProvider>();
            mockSchoolYearProvider.Stub(x => x.GetSchoolYear()).Return(DateTime.Now.Year);
            var action = default(Func<IBulkOperationDbContext, UploadFile>);
            mockDb.Stub(x => x.Get(action)).IgnoreArguments().Return(new UploadFile{Id = uploadId, Size = (offset + size) });
            mockDb.Stub(x => x.Get(default(Func<IBulkOperationDbContext, BulkOperation>)))
                .IgnoreArguments()
                .Return(new BulkOperation { SchoolYear = DateTime.Now.Year }); 
            var sut = new UploadValidator(mockDb, mockSchoolYearProvider); 
            var validationErrors = sut.IsValid(uploadId, offset, size);
            validationErrors.Any.ShouldBeFalse();
        }

        [Test]
        public void And_The_Upload_Exists_And_Offset_Plus_Size_Exceeds_Total_File_Size_Should_Return_Errors()
        {
            var uploadId = Guid.NewGuid().ToString();
            var offset = 0;
            var size = 10000000;
            var mockDb = this.Stub<IDbExecutor<IBulkOperationDbContext>>();
            var mockSchoolYearProvider = this.Stub<ISchoolYearContextProvider>();
            mockSchoolYearProvider.Stub(x => x.GetSchoolYear()).Return(DateTime.Now.Year);
            var action = default(Func<IBulkOperationDbContext, UploadFile>);
            mockDb.Stub(x => x.Get(action)).IgnoreArguments().Return(new UploadFile{Id = uploadId, Size = offset });
            mockDb.Stub(x => x.Get(default(Func<IBulkOperationDbContext, BulkOperation>)))
                .IgnoreArguments()
                .Return(new BulkOperation { SchoolYear = DateTime.Now.Year });
            var sut = new UploadValidator(mockDb, mockSchoolYearProvider);
            var validationErrors = sut.IsValid(uploadId, offset, size);
            validationErrors.Any.ShouldBeTrue();
            validationErrors.ErrorMessage.ShouldContain("size");
        }

        [Test]
        public void And_The_Upload_Does_Not_Exist_Should_Return_Errors()
        {
            var mockDb = this.Stub<IDbExecutor<IBulkOperationDbContext>>();
            var action = default(Func<IBulkOperationDbContext, UploadFile>);
            mockDb.Stub(x => x.Get(action)).IgnoreArguments().Return(new UploadFile());
            var mockSchoolYearProvider = this.Stub<ISchoolYearContextProvider>();
            mockSchoolYearProvider.Stub(x => x.GetSchoolYear()).Return(DateTime.Now.Year);
            var bogusUploadId = Guid.NewGuid().ToString();
            mockDb.Stub(x => x.Get(default(Func<IBulkOperationDbContext, BulkOperation>)))
                .IgnoreArguments()
                .Return(new BulkOperation {SchoolYear = DateTime.Now.Year});
            var sut = new UploadValidator(mockDb, mockSchoolYearProvider);
            var validationErrors = sut.IsValid(bogusUploadId, 100000, 100000);
            validationErrors.Any.ShouldBeTrue();
            validationErrors.ErrorMessage.ShouldContain(bogusUploadId);
        }
    }
}