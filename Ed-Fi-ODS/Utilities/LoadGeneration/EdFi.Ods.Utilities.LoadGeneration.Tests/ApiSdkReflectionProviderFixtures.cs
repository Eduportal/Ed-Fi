using System;
using System.Reflection;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Tests.ConventionTestClasses.Api;
using EdFi.Ods.Utilities.LoadGeneration.Tests.ConventionTestClasses.Models;
using EdFi.Ods.Utilities.LoadGeneration.Tests.ConventionTestClasses.Sdk;
using NUnit.Framework;
using RestSharp;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public class ApiSdkReflectionProviderFixtures
    {
        public class When_getting_api_constructor : TestFixtureBase
        {
            private IApiSdkReflectionProvider apiSdkReflectionProvider;
            private ConstructorInfo result;

            protected override void Arrange()
            {
                apiSdkReflectionProvider = new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider());
            }

            protected override void Act()
            {
                result = apiSdkReflectionProvider.GetApiConstructorForModelType(typeof(GradeLevelDescriptor));
            }

            [Test]
            public void Should_be_declared_in_the_correct_type()
            {
                Assert.That(result.DeclaringType, Is.EqualTo(typeof (GradeLevelDescriptorsApi)));
            }
        }

        public class When_getting_the_GetAllMethod_from_the_api_object : TestFixtureBase
        {
            private IApiSdkReflectionProvider apiSdkReflectionProvider;
            private GradeLevelDescriptorsApi api;
            private MethodInfo result;
            
            protected override void Arrange()
            {
                var restClient = MockRepository.GenerateMock<IRestClient>();
                api = new GradeLevelDescriptorsApi(restClient);

                apiSdkReflectionProvider = new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider());
            }

            protected override void Act()
            {
                result = apiSdkReflectionProvider.LocateGetAllMethodFrom(api, typeof(GradeLevelDescriptor));
            }

            [Test]
            public void Should_have_the_correct_name()
            {
                Assert.That(result.Name, Is.EqualTo("GetGradeLevelDescriptorsAll"));
            }
        }

        public class When_getting_the_PostMethod_from_the_api_object : TestFixtureBase
        {
            private IApiSdkReflectionProvider apiSdkReflectionProvider;
            private GradeLevelDescriptorsApi api;
            private MethodInfo result;

            protected override void Arrange()
            {
                var restClient = MockRepository.GenerateMock<IRestClient>();
                api = new GradeLevelDescriptorsApi(restClient);

                apiSdkReflectionProvider = new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider());
            }

            protected override void Act()
            {
                result = apiSdkReflectionProvider.LocatePostMethodFrom(api);
            }

            [Test]
            public void Should_have_the_correct_name()
            {
                Assert.That(result.Name, Is.EqualTo("PostGradeLevelDescriptors"));
            }
        }

        public class When_searching_for_a_model_by_name : TestFixtureBase
        {
            private IApiSdkReflectionProvider apiSdkReflectionProvider;
            private Type _actualModelType;
            private bool _actualResult;

            protected override void Arrange()
            {
                apiSdkReflectionProvider = new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider());
            }

            protected override void Act()
            {
                //The type name is intentionally cased wrong on this, so we can make sure it handles incorrect casing too.
                _actualResult = apiSdkReflectionProvider.TryGetModelType("grAdELevelDescriptor", out _actualModelType);
            }

            [Test]
            public void Should_identify_the_model_type_based_on_a_case_insensitive_match_on_type_name()
            {
                Assert.That(_actualModelType, Is.EqualTo(typeof(GradeLevelDescriptor)));
            }
        }

        public class When_searching_for_a_model_by_reference_type : TestFixtureBase
        {
            private IApiSdkReflectionProvider apiSdkReflectionProvider;
            private Type _actualModelType;
            private bool _actualResult;

            protected override void Arrange()
            {
                apiSdkReflectionProvider = new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider());
            }

            protected override void Act()
            {
                _actualResult = apiSdkReflectionProvider.TryGetModelType(typeof(CompetencyObjectiveReference), out _actualModelType);
            }

            [Test]
            public void Should_locate_corresponding_model_type_by_trimming_reference_suffix_from_the_type_name()
            {
                Assert.That(_actualModelType, Is.EqualTo(typeof(CompetencyObjective)));
            }
        }

        public class When_searching_for_a_reference_type_by_model_type : TestFixtureBase
        {
            private IApiSdkReflectionProvider apiSdkReflectionProvider;
            private Type _actualReferenceType;

            protected override void Arrange()
            {
                apiSdkReflectionProvider = new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider());
            }

            protected override void Act()
            {
                apiSdkReflectionProvider.TryGetReferenceType(typeof(CompetencyObjective), out _actualReferenceType);
            }

            [Test]
            public void Should_locate_corresponding_reference_type_by_adding_reference_as_a_suffix_to_the_type_name()
            {
                Assert.That(_actualReferenceType, Is.EqualTo(typeof(CompetencyObjectiveReference)));
            }
        }

        public class When_searching_for_a_reference_type_by_model_type_where_the_type_passed_in_has_no_corresponding_reference_type : TestFixtureBase
        {
            private IApiSdkReflectionProvider apiSdkReflectionProvider;
            private bool _actualResult;
            private Type _actualReferenceType;

            protected override void Arrange()
            {
                apiSdkReflectionProvider = new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider());
            }

            protected override void Act()
            {
                _actualResult = apiSdkReflectionProvider.TryGetReferenceType(typeof(GradeLevelDescriptor), out _actualReferenceType);
            }

            [Assert]
            public void Should_indicate_a_reference_type_could_not_be_located()
            {
                _actualResult.ShouldBeFalse();
            }

            [Assert]
            public void Should_return_a_null_reference_type()
            {
                _actualReferenceType.ShouldBeNull();
            }
        }

        public class When_searching_for_a_model_by_reference_type_where_the_type_passed_in_is_not_a_reference_type : TestFixtureBase
        {
            private IApiSdkReflectionProvider apiSdkReflectionProvider;
            private Type _actualModelType;
            private Exception actualException;
            private bool _actualResult;

            protected override void Arrange()
            {
                apiSdkReflectionProvider = new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider());
            }

            protected override void Act()
            {
                try
                {
                    _actualResult = apiSdkReflectionProvider.TryGetModelType(typeof(GradeLevelDescriptor), out _actualModelType);
                }
                catch (Exception ex)
                {
                    actualException = ex;
                }
            }

            [Test]
            public void Should_throw_an_exception_indicating_that_the_reference_type_provided_isnt_a_reference_type()
            {
                actualException.ShouldNotBeNull();
                actualException.Message.ShouldContain("is not a reference type");
            }
        }

        public class When_Searching_for_a_model_that_doesnt_exist : TestFixtureBase
        {
            private IApiSdkReflectionProvider apiSdkReflectionProvider;
            private Type _actualModelType;
            private Exception _actualException;
            private bool _actualResult;

            protected override void Arrange()
            {
                apiSdkReflectionProvider = new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider());
            }

            protected override void Act()
            {
                _actualResult = apiSdkReflectionProvider.TryGetModelType("asdfasdfadsfasdfasd", out _actualModelType);
            }

            [Assert]
            public void Should_indicate_failure()
            {
                _actualResult.ShouldBeFalse();
            }

            [Test]
            public void Should_return_a_null_model_type()
            {
                _actualModelType.ShouldBeNull();
            }
        }

        public class When_trying_to_get_the_TokenRetrieverConstructor : TestFixtureBase
        {
            private IApiSdkReflectionProvider apiSdkReflectionProvider;
            private ConstructorInfo result;

            protected override void Arrange()
            {
                apiSdkReflectionProvider = new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider());
            }

            protected override void Act()
            {
                result = apiSdkReflectionProvider.GetTokenRetrieverConstructor();
            }

            [Test]
            public void Should_belong_to_the_correct_type()
            {
                Assert.That(result.DeclaringType, Is.EqualTo(typeof(TokenRetriever)));
            }
        }

        public class When_trying_to_get_the_BearerTokenAuthenticator : TestFixtureBase
        {
            private IApiSdkReflectionProvider apiSdkReflectionProvider;
            private ConstructorInfo result;

            protected override void Arrange()
            {
                apiSdkReflectionProvider = new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider());
            }

            protected override void Act()
            {
                result = apiSdkReflectionProvider.GetBearerTokenAuthenticatorConstructor();
            }

            [Test]
            public void Should_belong_to_the_correct_type()
            {
                Assert.That(result.DeclaringType, Is.EqualTo(typeof(BearerTokenAuthenticator)));
            }
        }
    }
}