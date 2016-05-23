using System;
using System.Collections.Generic;
using EdFi.Ods.Tests._Bases;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public class ResourceReferenceCollectionFixtures
    {
        private class When_using_ResourceReferenceCollection_with_a_empty_collection : TestFixtureBase
        {
            [Assert]
            public void Should_return_null()
            {
                var collection = new ExistingResourceReferenceProvider.ResourceReferenceCollection(typeof(object), new PropertyConstraintsCollectionFilter());
                var actualResult = collection.GetRandomMember();
                actualResult.ShouldBeNull();
            }

            [Assert]
            public void Should_return_null_when_supplying_property_value_constraints()
            {
                var collection = new ExistingResourceReferenceProvider.ResourceReferenceCollection(typeof(object), new PropertyConstraintsCollectionFilter());
                var suppliedpropertyValueConstraints = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                var actualResult = collection.GetRandomMember(suppliedpropertyValueConstraints);
                actualResult.ShouldBeNull();
            }
        }
    }
}
