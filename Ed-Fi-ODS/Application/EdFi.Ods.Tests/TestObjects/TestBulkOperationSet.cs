namespace EdFi.Ods.Tests.TestObjects
{
    using System.Linq;

    using global::EdFi.Ods.Api.Data;

    public class TestBulkOperationSet : TestDbSet<BulkOperation>
    {
        public override BulkOperation Find(params object[] keyValues)
        {
            var id = (string) keyValues.Single();
            return this.SingleOrDefault(b => b.Id == id);
        }
    }
}