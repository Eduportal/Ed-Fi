using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Security;
using EdFi.Ods.Utilities.LoadGeneration.Tests.ConventionTestClasses.Models;
using NUnit.Framework;
using RestSharp;
using Rhino.Mocks;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public class ApiWrapperFixtures
    {
        [Ignore("This test hits an actual server.  This should be a manual process or in a category that doesn't get run by default.")]
        public class When_getting_all_from_the_real_rest_api : TestFixtureBase
        {
            private List<GradeLevelDescriptor> results;
            private IApiSdkFacade apiSdkFacade;

            protected override void Arrange()
            {
                apiSdkFacade = new ApiSdkFacade(new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider()),
                    new RestClientPool(new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider()), new TestApiSecurityContextProvider()));
            }

            protected override void Act()
            {
                results = apiSdkFacade.GetAll(typeof(GradeLevelDescriptor))
                    .Cast<GradeLevelDescriptor>()
                    .ToList();
            }

            [Test]
            public void Should_get_multiple_distict_values()
            {
                var distinctCodes = results.Select(res => res.codeValue).Distinct().ToList();

                //We should get multiple grades.
                Assert.That(distinctCodes, Has.Count.GreaterThan(5));
                Assert.That(distinctCodes, Has.Count.EqualTo(results.Count));
            }
        }

        public class When_getting_all_from_a_mock_of_the_api : TestFixtureBase
        {
            private IRestClientPool _restApiClientPool;
            private ApiSdkFacade _apiSdkFacade;
            private IRestClient restClient;
            private IRestResponse<List<GradeLevelDescriptor>> restResponse;
            private IEnumerable results;

            protected override void Arrange()
            {
                _restApiClientPool = Stub<IRestClientPool>();
                restClient = Stub<IRestClient>();

                restResponse = new RestResponse<List<GradeLevelDescriptor>>() { StatusCode = HttpStatusCode.OK };
                restResponse.Data = new List<GradeLevelDescriptor>
                                        {
                                            new GradeLevelDescriptor{codeValue = "Test1"},
                                            new GradeLevelDescriptor{codeValue = "Test2"},
                                            new GradeLevelDescriptor{codeValue = "Test3"}
                                        };

                restClient.Expect(client => client.Execute<List<GradeLevelDescriptor>>(null))
                    .IgnoreArguments()
                    .Return(restResponse);
                
                _restApiClientPool.Expect(x => x.GetRestClient(true)).IgnoreArguments().Return(restClient);

                _apiSdkFacade = new ApiSdkFacade(new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider()), _restApiClientPool);
            }

            protected override void Act()
            {
                results = _apiSdkFacade.GetAll(typeof(GradeLevelDescriptor));
            }

            [Test]
            public void Should_return_the_correct_number_of_results()
            {
                Assert.That(results, Has.Count.EqualTo(3));
            }

            [Test]
            public void Should_have_accessed_api_for_data()
            {
                restClient.AssertWasCalled(client => 
                    client.Execute<List<GradeLevelDescriptor>>(Arg<IRestRequest>.Is.Anything));
            }
        }

        [Ignore("This test hits an actual server.  This should be a manual process or in a category that doesn't get run by default.")]
        public class When_Posting_bad_data_to_the_api : TestFixtureBase
        {
            private IRestResponse result;
            private IApiSdkFacade apiSdkFacade;
            private ClassPeriod itemToPost;

            private const int InvalidSchoolId = -38453923;
            private const string ClassPeriodName = "LoadGeneration.Tests";

            protected override void Arrange()
            {
                apiSdkFacade = new ApiSdkFacade(new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider()),
                    new RestClientPool(new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider()), new TestApiSecurityContextProvider()));

                itemToPost = new ClassPeriod { schoolReference = new SchoolReference { schoolId = InvalidSchoolId }, name = ClassPeriodName };
            }

            protected override void Act()
            {
                result = apiSdkFacade.Post(itemToPost);
            }

            [Test]
            public void then_should_get_a_failure_back_from_the_api()
            {
                //Try posting data that refers to a school that doesn't exist.  Since our goal is to test to see if we're calling to the API,
                //  Not to test the API itself, this lets us test to make sure we can post to the API, without actually modifying any data on it.

                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                Assert.That(result.Content, Is.StringContaining("The claim does not have any established relationships with the requested resource."));
            }
        }

        public class When_Posting_to_a_mock_of_the_api : TestFixtureBase
        {
            private IRestResponse result;
            private IApiSdkFacade apiSdkFacade;
            private ClassPeriod itemToPost;
            private IRestClient restClient;
            private IRestClientPool _restApiClientPool;
            private RestResponse restResponse;

            private const int InvalidSchoolId = -38453923;
            private const string ClassPeriodName = "LoadGeneration.Tests";

            protected override void Arrange()
            {
                _restApiClientPool = Stub<IRestClientPool>();
                restClient = Stub<IRestClient>();

                restResponse = new RestResponse();

                restClient.Expect(client => client.Execute(null)).IgnoreArguments().Return(restResponse);

                _restApiClientPool.Expect(x => x.GetRestClient(true)).IgnoreArguments().Return(restClient);

                apiSdkFacade = new ApiSdkFacade(new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider()), _restApiClientPool);

                itemToPost = new ClassPeriod { schoolReference = new SchoolReference { schoolId = InvalidSchoolId }, name = ClassPeriodName };
            }

            protected override void Act()
            {
                result = apiSdkFacade.Post(itemToPost);
            }

            [Test]
            public void then_should_get_back_the_result()
            {
                Assert.That(result, Is.EqualTo(restResponse));
            }
        }


    }
}