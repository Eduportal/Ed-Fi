using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Api.Data.Contexts;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using System.Data.Entity;

namespace EdFi.Ods.Api.Data.Tests
{
    [TestFixture]
    public class When_inserting_a_bulk_operation_entity_and_reading_it_from_database
    {
        private BulkOperation insertedBulkOperation;
        private BulkOperation actualBulkOperation;
        private List<Difference> _differences;

        [SetUp]
        public void SetUp()
        {
            insertedBulkOperation = BulkOperationEfHelper.InsertSampleBulkOperation();
            
            using (var context = new BulkOperationDbContext())
            {
                actualBulkOperation = context.BulkOperations.AsQueryable()
                    .Include("UploadFiles")
                    .Single(x => x.Id == insertedBulkOperation.Id);
                var comparer = new CompareObjects { IgnoreObjectTypes = true };

                comparer.Compare(insertedBulkOperation, actualBulkOperation);

                _differences = comparer.Differences;
            }    
        }

        [Test]
        public void Read_entity_should_match_inserted_entity()
        {
            foreach (var differnce in _differences)
            {
                Console.WriteLine("Property {0} difference, expected: {1}, actual: {2}", differnce.PropertyName,
                                  differnce.Object1Value, differnce.Object2Value);
            }
        }
    }
}
