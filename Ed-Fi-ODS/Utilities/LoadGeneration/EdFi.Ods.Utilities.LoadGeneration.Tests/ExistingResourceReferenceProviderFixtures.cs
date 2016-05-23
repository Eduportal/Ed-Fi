using System;
using System.Collections.Generic;
using EdFi.Ods.Tests._Bases;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public class ExistingResourceReferenceProviderFixtures
    {
        public class ResourceReferenceTestObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class OtherResourceReferenceTestObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class When_adding_an_existing_resource_reference : TestFixtureBase
        {
            private IExistingResourceReferenceProvider ExistingResourceReferenceProvider;
            private ResourceReferenceTestObject suppliedResourceReference;
            private ResourceReferenceTestObject suppliedResourceReference2;
            
            protected override void Arrange()
            {
                suppliedResourceReference = new ResourceReferenceTestObject { Id = 1, Name = "Foo" };
                suppliedResourceReference2 = new ResourceReferenceTestObject { Id = 2, Name = "Foo" };
            }

            protected override void Act()
            {
                ExistingResourceReferenceProvider = new ExistingResourceReferenceProvider(new PropertyConstraintsCollectionFilter());
                ExistingResourceReferenceProvider.AddResourceReference(suppliedResourceReference);
                ExistingResourceReferenceProvider.AddResourceReference(suppliedResourceReference2);
            }

            [Assert]
            public void Should_return_the_reference_when_requested_by_type()
            {
                var actualObject =
                    ExistingResourceReferenceProvider.GetResourceReference(typeof(ResourceReferenceTestObject), null) as ResourceReferenceTestObject;

                actualObject.ShouldNotBeNull();

                Assert.That(actualObject, 
                    Is.SameAs(suppliedResourceReference)
                    .Or.SameAs(suppliedResourceReference2));
            }

            [Assert]
            public void Should_return_null_when_requested_using_a_type_for_which_no_reference_has_been_added()
            {
                var actualObject = ExistingResourceReferenceProvider.GetResourceReference(typeof(OtherResourceReferenceTestObject), null);

                actualObject.ShouldBeNull();
            }
        }

        public class When_getting_an_existing_resource_reference_with_property_value_constraints : TestFixtureBase
        {
            private IExistingResourceReferenceProvider ExistingResourceReferenceProvider;
            private List<object> suppliedResourceReferences;
            private object actualResourceForIdOf2;
            private Dictionary<string, object> suppliedPropertyValueConstraints;
            private object actualResourceForNameOfFoo;
            private object actualResourceForIdOf2AndNameOfFoo;
            private object actualResourceForIdOf2AndNonExistingProperty;

            protected override void Arrange()
            {
                suppliedResourceReferences = new List<object>
                {
                    new ResourceReferenceTestObject {Id = 1, Name = "Foo"},
                    new ResourceReferenceTestObject {Id = 2, Name = "Bar"},
                    new OtherResourceReferenceTestObject {Id = 3, Name = "Baz"},
                };
            }

            protected override void Act()
            {
                ExistingResourceReferenceProvider = new ExistingResourceReferenceProvider(new PropertyConstraintsCollectionFilter());

                foreach (var suppliedResourceReference in suppliedResourceReferences)
                {
                    ExistingResourceReferenceProvider.AddResourceReference(suppliedResourceReference);
                }

                actualResourceForIdOf2 = ExistingResourceReferenceProvider.GetResourceReference(
                    typeof(ResourceReferenceTestObject),
                    new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
                    {
                        {"Id",2}
                    });

                actualResourceForNameOfFoo = ExistingResourceReferenceProvider.GetResourceReference(
                    typeof(ResourceReferenceTestObject),
                    new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
                    {
                        {"Name", "Foo"}
                    });

                actualResourceForIdOf2AndNameOfFoo = ExistingResourceReferenceProvider.GetResourceReference(
                    typeof(ResourceReferenceTestObject),
                    new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
                    {
                        {"Id", 2},
                        {"Name", "Foo"},
                    });

                actualResourceForIdOf2AndNonExistingProperty = ExistingResourceReferenceProvider.GetResourceReference(
                    typeof(ResourceReferenceTestObject),
                    new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
                    {
                        {"Id", 2},
                        {"NonExistingProperty", "Foo"},
                    });


            }

            [Assert]
            public void Should_return_the_matching_resource_reference_when_the_corresponding_type_and_an_individual_matching_constraint_are_supplied()
            {
                actualResourceForIdOf2.ShouldNotBeNull();
                actualResourceForIdOf2.GetType().ShouldEqual(typeof(ResourceReferenceTestObject));
                var actualConcrete = (ResourceReferenceTestObject)actualResourceForIdOf2;
                actualConcrete.Id.ShouldEqual(2);
                actualConcrete.Name.ShouldEqual("Bar");
            }

            [Assert]
            public void Should_return_the_matching_resource_reference_when_the_corresponding_type_and_a_different_individual_matching_constraint_are_supplied()
            {
                actualResourceForNameOfFoo.ShouldNotBeNull();
                actualResourceForNameOfFoo.GetType().ShouldEqual(typeof(ResourceReferenceTestObject));

                var actualConcrete = (ResourceReferenceTestObject)actualResourceForNameOfFoo;
                actualConcrete.Id.ShouldEqual(1);
                actualConcrete.Name.ShouldEqual("Foo");
            }

            [Assert]
            public void Should_return_null_when_the_provided_constraints_do_not_match_all_existing_properties()
            {
                actualResourceForIdOf2AndNameOfFoo.ShouldBeNull();
            }

            [Assert]
            public void Should_return_the_matching_resource_reference_as_expected_when_a_property_value_constraint_matches_even_when_another_constraint_does_not_exist_on_the_reference()
            {
                actualResourceForIdOf2AndNonExistingProperty.ShouldNotBeNull();
                actualResourceForIdOf2AndNonExistingProperty.GetType().ShouldEqual(typeof(ResourceReferenceTestObject));

                var actualConcrete = (ResourceReferenceTestObject)actualResourceForIdOf2AndNonExistingProperty;
                actualConcrete.Id.ShouldEqual(2);
                actualConcrete.Name.ShouldEqual("Bar");
            }
        }
    }
}
