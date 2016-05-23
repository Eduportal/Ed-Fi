namespace EdFi.Ods.Tests.EdFi.Ods.CommonUtils.DataStructures
{
    using System.Collections.Generic;
    using System.Linq;

    using global::EdFi.Ods.BulkLoad.Common;

    using NUnit.Framework;

    using Should;

    public class OrderedCollectionOfSetsTests
    {
        [TestFixture]
        public class When_structure_is_populated_with_3_sets_that_each_contain_3_elements
        {
            private OrderedCollectionOfSets<string> _structure;

            [TestFixtureSetUp]
            public void Setup()
            {
                this._structure = new OrderedCollectionOfSets<string>();

                this._structure
                    .AddSet().AddMember("foo 1").AddMember("bar 1").AddMember("baz 1")
                    .AddSet().AddMember("foo 2").AddMember("bar 2").AddMember("baz 2")
                    .AddSet().AddMemberRange(new []{"foo 3", "bar 3","baz 3"});
            }

            [Test]
            public void Enumerating_the_structure_should_produce_the_sets_in_order()
            {
                ISet<string>[] results = this._structure.ToArray();
                results.Length.ShouldEqual(3);
                results[0].ShouldContain("foo 1");
                results[1].ShouldContain("foo 2");
                results[2].ShouldContain("foo 3");
            }

            [Test]
            public void Should_each_set_should_contain_the_correct_elements()
            {
                ISet<string>[] results = this._structure.ToArray();
                results.Length.ShouldEqual(3);
                results[0].ShouldContain("foo 1");
                results[0].ShouldContain("bar 1");
                results[0].ShouldContain("baz 1");

                results[1].ShouldContain("foo 2");
                results[1].ShouldContain("bar 2");
                results[1].ShouldContain("baz 2");
                
                results[2].ShouldContain("foo 3");
                results[2].ShouldContain("bar 3");
                results[2].ShouldContain("baz 3");
            }
        } 
    }
}