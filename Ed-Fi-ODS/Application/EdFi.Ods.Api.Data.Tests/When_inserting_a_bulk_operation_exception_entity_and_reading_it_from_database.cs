using System;
using System.Data.Entity.Validation;
using System.Linq;
using EdFi.Ods.Api.Data.Contexts;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;

namespace EdFi.Ods.Api.Data.Tests
{
    [TestFixture]
    public class When_inserting_a_bulk_operation_exception_entity_and_reading_it_from_database
    {
        private BulkOperation _bulkOperation;
        private string _parentUploadFileId;
        private BulkOperationException _insertedBulkOperationException;
        private BulkOperationException _actualBulkOperationException;

        [TestFixtureSetUp]
        public void Setup()
        {
            _bulkOperation = BulkOperationEfHelper.InsertSampleBulkOperation();
            _parentUploadFileId = _bulkOperation.UploadFiles.First().Id;
            _insertedBulkOperationException = new BulkOperationException
            {
                Element = "Some element",
                Message = "Something failed!",
                Code = 100,
                Type = Guid.NewGuid().ToString(),
                ParentUploadFileId = _parentUploadFileId,
                DateTime = DateTime.Now,
                StackTrace = "stackTrace"
            };


            using (var context = new BulkOperationDbContext())
            {
                context.BulkOperationExceptions.Add(_insertedBulkOperationException);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    BulkOperationEfHelper.WriteEfValidationErrorsToConsole(e);
                    throw;
                }
            }

            using (var context = new BulkOperationDbContext())
            {
                _actualBulkOperationException = context.BulkOperationExceptions.Single(x => x.Type == _insertedBulkOperationException.Type);
            }
        }

        [Test]
        public void Read_entity_should_match_inserted_entity()
        {
            var comparer = new CompareObjects { IgnoreObjectTypes = true };
            //Ignore 'ParentUploadFile' property (we don't want the compare to trigger the lazy load)
            comparer.ElementsToIgnore.Add("ParentUploadFile");
            comparer.ElementsToIgnore.Add("DateTime");//<==For reasons of DB data type translation
            comparer.Compare(_insertedBulkOperationException, _actualBulkOperationException);

            Assert.AreEqual(0, comparer.Differences.Count);

            foreach (var differnce in comparer.Differences)
            {
                Console.WriteLine("Property {0} difference, expected: {1}, actual: {2}", differnce.PropertyName,
                    differnce.Object1Value, differnce.Object2Value);
            }
        }
    }
}