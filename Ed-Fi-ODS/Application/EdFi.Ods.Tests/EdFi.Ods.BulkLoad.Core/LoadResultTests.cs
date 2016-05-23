namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core
{
    using global::EdFi.Ods.BulkLoad.Core;

    using NUnit.Framework;

    using Should;

    public class LoadResultTests
    {
        [TestFixture]
        public class When_constructed_with_defaults
        {
            [Test]
            public void Should_have_zero_load_exceptions()
            {
                var result = new LoadResult();
                result.LoadExceptions.Count.ShouldEqual(0);
            }
        } 
    }
}