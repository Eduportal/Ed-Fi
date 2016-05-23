using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Tests.ConventionTestClasses.Models;
using NUnit.Framework;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public class DescriptorFactoryFixtures
    {
        //This is also used to verify that GetNext(Type) calls off to GetNext<T>()
        public class When_trying_To_access_a_descriptor_that_has_no_data : TestFixtureBase
        {
            private SequentialItemFromApiProvider sequentialItemFromApiProvider;
            private object _actualResult;

            protected override void Arrange()
            {
                var descriptorService = MockRepository.GenerateMock<IApiSdkFacade>();

                descriptorService.Expect(ds => ds.GetAll(typeof(GradeLevelDescriptor)))
                                 .Return(new List<GradeLevelDescriptor>());

                sequentialItemFromApiProvider = new SequentialItemFromApiProvider(descriptorService);
                _actualResult = sequentialItemFromApiProvider.GetNext(typeof(GradeLevelDescriptor));
            }

            [Test]
            public void Should_fail_due_to_lack_of_data()
            {
                _actualResult.ShouldBeNull();
            }

            //[Test]
            //public void Should_fail_due_to_lack_of_data()
            //{
            //    Assert.Throws<TargetInvocationException>(
            //        () => sequentialItemFromApiProvider.GetNext(typeof (GradeLevelDescriptor)));
            //}
        }

        public class When_trying_To_access_a_descriptor_with_generics_that_has_no_data : TestFixtureBase
        {
            private SequentialItemFromApiProvider sequentialItemFromApiProvider;
            private Exception actualException;
            private GradeLevelDescriptor _actualResult;

            protected override void Arrange()
            {
                var descriptorService = MockRepository.GenerateMock<IApiSdkFacade>();

                descriptorService.Expect(ds => ds.GetAll(typeof(GradeLevelDescriptor)))
                                 .Return(new List<GradeLevelDescriptor>());

                sequentialItemFromApiProvider = new SequentialItemFromApiProvider(descriptorService);
            }

            protected override void Act()
            {
                try
                {
                    _actualResult = sequentialItemFromApiProvider.GetNext<GradeLevelDescriptor>();
                }
                catch (Exception ex)
                {
                    actualException = ex;
                }
            }

            //[Test]
            //public void Should_throw_an_exception()
            //{
            //    Assert.That(actualException, Is.TypeOf<Exception>());
            //}

            [Assert]
            public void Should_return_a_null_vaue()
            {
                _actualResult.ShouldBeNull();
            }
        }

        public class When_pulling_multiple_values : TestFixtureBase
        {
            const int TestLength = 100;
            private IApiSdkFacade apiSdkFacade;
            private List<GradeLevelDescriptor> results = new List<GradeLevelDescriptor>();
            private List<GradeLevelDescriptor> testData;

            private SequentialItemFromApiProvider sequentialItemFromApiProvider;

            protected override void Arrange()
            {
                testData = new List<GradeLevelDescriptor>
                               {
                                   new GradeLevelDescriptor{codeValue = "1"},
                                   new GradeLevelDescriptor{codeValue = "2"},
                                   new GradeLevelDescriptor{codeValue = "3"},
                                   new GradeLevelDescriptor{codeValue = "4"},
                               };

                

                apiSdkFacade = MockRepository.GenerateMock<IApiSdkFacade>();

                apiSdkFacade.Expect(ds => ds.GetAll(typeof(GradeLevelDescriptor))).Return(testData);

                sequentialItemFromApiProvider = new SequentialItemFromApiProvider(apiSdkFacade);
            }

            protected override void Act()
            {
                for (int i = 0; i < TestLength; i++)
                {
                    results.Add(sequentialItemFromApiProvider.GetNext<GradeLevelDescriptor>());
                }
            }

            [Test]
            public void Should_return_the_same_number_of_values_as_we_called_it()
            {
                //Prove that we've returned the same number of values that we asked for
                Assert.That(results, Has.Count.EqualTo(TestLength));
            }

            [Test]
            public void Should_only_return_the_test_data()
            {
                //Prove that we're returning the same 4 values
                var distinctCodes = results.Select(res => res.codeValue).Distinct().ToList();
                Assert.That(distinctCodes, Has.Count.EqualTo(testData.Count));
            }
        }
    }
}
