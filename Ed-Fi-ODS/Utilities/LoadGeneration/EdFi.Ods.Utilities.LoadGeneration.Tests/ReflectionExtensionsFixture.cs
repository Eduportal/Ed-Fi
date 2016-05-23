using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Tests._Bases;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public class ReflectionExtensionsFixture
    {
        public class TestResource
        {
            public TestResource(int id)
            {
                Id = id;
            }

            public TestResource(int id, string name)
            {
                Name = name;
                Id = id;
            }

            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class When_using_HasProperty : TestFixtureBase
        {
            [Assert]
            public void Should_return_true_when_the_property_exists()
            {
                var suppliedResource = new TestResource(1);
                suppliedResource.HasProperty("Id").ShouldBeTrue();
            }

            [Assert]
            public void Should_return_false_when_the_property_does_not_exist()
            {
                var suppliedResource = new TestResource(1);
                suppliedResource.HasProperty("NotThere").ShouldBeFalse();
            }
        }

        private class When_using_HasPropertyThatMatchesValue_with_a_property_and_value_that_exist : TestFixtureBase
        {
            [Assert]
            public void Should_return_true()
            {
                var suppliedResource = new TestResource(1);
                suppliedResource.HasPropertyThatMatchesValue("Id", 1).ShouldBeTrue();
            }
        }

        private class When_using_HasPropertyThatMatchesValue_with_a_property_that_exists_and_value_that_does_NOT_exist : TestFixtureBase
        {
            [Assert]
            public void Should_return_false()
            {
                var suppliedResource = new TestResource(1);
                suppliedResource.HasPropertyThatMatchesValue("Id", 2).ShouldBeFalse();
            }
        }

        private class When_using_HasPropertyThatMatchesValue_with_a_property_that_does_NOT_exist : TestFixtureBase
        {
            [Assert]
            public void Should_return_false_and_not_throw_exception()
            {
                var suppliedResource = new TestResource(1);
                var result = suppliedResource.HasPropertyThatMatchesValue("PropertyNameThatDoesNOTExist", 1);

                result.ShouldBeFalse();
            }
        }

        private class When_using_WhereObjectsMatchPropertyValueConstraints : TestFixtureBase
        {
            [Assert]
            public void Should_return_one_test_resource_that_matches_the_Property_value_constraints()
            {
                var suppliedResources = new List<TestResource>
                {
                    new TestResource(1),
                    new TestResource(2),
                };

                var suppliedPropertyValueConstraints = new Dictionary<string, object>
                {
                    {"Id",1},
                };

                var actualList = suppliedResources.WhereObjectsMatchPropertyValueConstraints(suppliedPropertyValueConstraints).ToList();
                actualList.Count().ShouldEqual(1);
                ((TestResource)actualList[0]).Id.ShouldEqual(1);
            }

            [Assert]
            public void Should_return_two_test_resource_that_match_the_property_value_constraints()
            {
                var suppliedResources = new List<TestResource>
                {
                    new TestResource(1,"a"),
                    new TestResource(1,"a"),
                    new TestResource(1,"Should Get Filtered Out"),
                    new TestResource(2,"Should Get Filtered Out"),
                };

                var suppliedPropertyValueConstraints = new Dictionary<string, object>
                {
                    {"Id",1},
                    {"Name","a"},
                };

                var actualList = suppliedResources.WhereObjectsMatchPropertyValueConstraints(suppliedPropertyValueConstraints).ToList();
                actualList.Count().ShouldEqual(2);
                var firstActualResource = ((TestResource) actualList[0]);
                var secondActualResource = ((TestResource) actualList[1]);

                firstActualResource.Id.ShouldEqual(1);
                firstActualResource.Name.ShouldEqual("a");

                secondActualResource.Id.ShouldEqual(1);
                secondActualResource.Name.ShouldEqual("a");
            }
        }
    }
}
