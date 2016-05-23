using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Tests.ConventionTestClasses.Models;
using NUnit.Framework;
using Rhino.Mocks;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public class SequentialItemFromApiProviderFixture
    {
        public class When_trying_to_get_items_from_api : TestFixtureBase
        {
            private const int numberOfTimesToCallApi = 6;
            private const int numberOfTestDescriptors = 2;
            private IApiSdkFacade apiSdkFacade;
            private GradeLevelDescriptor firstGradeLevelDescriptor = new GradeLevelDescriptor();
            private GradeLevelDescriptor secondGradeLevelDescriptor = new GradeLevelDescriptor();
            private SequentialItemFromApiProvider sequentialItemFromApiProvider;
            private List<GradeLevelDescriptor> results;

            protected override void Arrange()
            {
                apiSdkFacade = Stub<IApiSdkFacade>();
                apiSdkFacade.Expect(api => api.GetAll(typeof(GradeLevelDescriptor))).Return(
                    new[] {firstGradeLevelDescriptor, secondGradeLevelDescriptor});

                sequentialItemFromApiProvider = new SequentialItemFromApiProvider(apiSdkFacade);

                results = new List<GradeLevelDescriptor>();
            }


            protected override void Act()
            {
                for(int i = 0; i < numberOfTimesToCallApi; i++)
                    results.Add(sequentialItemFromApiProvider.GetNext<GradeLevelDescriptor>());
            }

            [Test]
            public void Should_Return_the_items_sequentially()
            {
                for (int i = 0; i < numberOfTimesToCallApi; i++)
                {
                    if (i%numberOfTestDescriptors == 0)
                    {
                        Assert.That(results[i], Is.EqualTo(firstGradeLevelDescriptor));
                    }
                    else
                    {
                        Assert.That(results[i], Is.EqualTo(secondGradeLevelDescriptor));
                    }
                }
            }

            [Test]
            public void Should_get_data_from_api_wrapper()
            {
                apiSdkFacade.AssertWasCalled(wrapper => wrapper.GetAll(typeof(GradeLevelDescriptor)));
            }
        }
    }
}
