namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using global::EdFi.Ods.Api.Data;

    using Rhino.Mocks;

    public class StubDbSetCreator
    {
        public static IDbSet<BulkOperation> CreateBulkOperations(string opId, List<UploadFile> uploadFiles)
        {
            var mockDbSet = MockRepository.GenerateStub<IDbSet<BulkOperation>>();

            var operation = new BulkOperation {Id = opId, UploadFiles = uploadFiles};
            var operations =
                new List<BulkOperation>
                {
                    operation
                }.AsQueryable();

            mockDbSet.Stub(m => m.Provider).Return(operations.Provider);
            mockDbSet.Stub(m => m.Expression).Return(operations.Expression);
            mockDbSet.Stub(m => m.GetEnumerator()).Return(operations.GetEnumerator());
            mockDbSet.Stub(m => m.Find(new object())).IgnoreArguments().Return(operation);

            return mockDbSet;
        }

        public static IDbSet<BulkOperation> CreateBulkOperations(string opId, List<UploadFile> uploadFiles, int schoolYear)
        {
            var mockDbSet = MockRepository.GenerateStub<IDbSet<BulkOperation>>();

            var operation = new BulkOperation {Id = opId, UploadFiles = uploadFiles, SchoolYear = schoolYear};
            var operations =
                new List<BulkOperation>
                {
                    operation
                }.AsQueryable();

            mockDbSet.Stub(m => m.Provider).Return(operations.Provider);
            mockDbSet.Stub(m => m.Expression).Return(operations.Expression);
            mockDbSet.Stub(m => m.GetEnumerator()).Return(operations.GetEnumerator());
            mockDbSet.Stub(m => m.Find(new object())).IgnoreArguments().Return(operation);

            return mockDbSet;
        }

        public static IDbSet<UploadFile> CreateUploadFiles(List<UploadFile> uploadFiles)
        {
            var mockDbSet = MockRepository.GenerateStub<IDbSet<UploadFile>>();

            var queryable = uploadFiles.AsQueryable();

            mockDbSet.Stub(m => m.Provider).Return(queryable.Provider);
            mockDbSet.Stub(m => m.Expression).Return(queryable.Expression);
            mockDbSet.Stub(m => m.GetEnumerator()).Return(queryable.GetEnumerator());
            mockDbSet.Stub(m => m.Find(new object())).IgnoreArguments().Return(uploadFiles.First());

            return mockDbSet;
        }

        public static IDbSet<BulkOperationException> CreateBulkOperationExceptions()
        {
            var mockDbSet = MockRepository.GenerateStub<IDbSet<BulkOperationException>>();

            var uploadFiles = new List<BulkOperationException>
            {
                new BulkOperationException
                {
                }
            }.AsQueryable();

            mockDbSet.Stub(m => m.Provider).Return(uploadFiles.Provider);
            mockDbSet.Stub(m => m.Expression).Return(uploadFiles.Expression);
            mockDbSet.Stub(m => m.GetEnumerator()).Return(uploadFiles.GetEnumerator());

            return mockDbSet;
        }
    }
}