using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Rhino.Mocks;
using Should;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Infrastructure;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    class When_selecting_a_resource_from_a_set_containing_counts_of_0_1_and_2 : TestFixtureBase
    {
        private class FakeRandom : IRandom
        {
            private int[] values;
            private int currentIndex = 0;
            public FakeRandom(int[] values)
            {
                this.values = values;
            }
            
            public int Next (int InclusiveLowerValue, int ExclusiveHigherValue)
            {
                int numberToReturn = values[currentIndex % values.Length];
                currentIndex++;
                return numberToReturn;
            }
        }
        
        // Supplied values

        // Actual values
        private string _actualSelectedResource1;
        private string _actualSelectedResource2;
        private string _actualSelectedResource3;

        // External dependencies
        private IResourceCountManager resourceCountManager;
        private IResourceSelector resourceSelector;

        protected override void Arrange()
        {
            // Set up mocked dependences and supplied values
            resourceCountManager = Stub<IResourceCountManager>();
            
            resourceCountManager
                .Expect(r => r.TotalRemainingCount)
                .Return(3);

            IEnumerable<ResourceCount> suppliedResources = new List<ResourceCount>
            {
                new ResourceCount("One", 0),
                new ResourceCount("Two", 1),
                new ResourceCount("Three", 2),
            };
            
            resourceCountManager
                .Expect(r => r.Resources)
                .Return(suppliedResources);

            var random = new FakeRandom(new int[] {1,2,3});
            resourceSelector = new ResourceSelector(resourceCountManager, random);
        }

        protected override void Act()
        {
            _actualSelectedResource1 = resourceSelector.GetNextResourceToGenerate();
            _actualSelectedResource2 = resourceSelector.GetNextResourceToGenerate();
            _actualSelectedResource3 = resourceSelector.GetNextResourceToGenerate();
        }

        [Assert]
        public void Should_skip_over_the_first_resource_when_the_random_number_is_1_since_it_has_no_instances_to_be_generated()
        {
            _actualSelectedResource1.ShouldNotEqual("One");
        }

        [Assert]
        public void Should_return_the_second_resource_when_the_random_number_is_1()
        {
            _actualSelectedResource1.ShouldEqual("Two");
        }

        [Assert]
        public void Should_return_the_third_resource_when_the_random_number_is_2()
        {
            _actualSelectedResource2.ShouldEqual("Three");
        }


        [Assert]
        public void Should_also_return_the_third_resource_when_the_random_number_is_3()
        {
            _actualSelectedResource3.ShouldEqual("Three");
        }
    }
}
