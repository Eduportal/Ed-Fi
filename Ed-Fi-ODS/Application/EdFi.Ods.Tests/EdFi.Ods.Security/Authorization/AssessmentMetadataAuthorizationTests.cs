using System.Collections.Generic;
using System.Security.Claims;
using EdFi.Common.Security;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Security.Authorization;
using EdFi.Ods.Security.AuthorizationStrategies.AssessmentMetadata;
using EdFi.Ods.Security.Claims;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Tests._Extensions;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.Security.Authorization
{
    public static class FeatureExtensions
    {
        public static IMethodOptions<string> that_given_an_assessment_title_of(
            this IAssessmentMetadataNamespaceProvider dependency,
            string assessmentTitle)
        {
            return dependency.Stub(x => x.GetNamespaceByAssessmentTitle(assessmentTitle));
        }
    }

    [TestFixture]
    public class AssessmentMetadataAuthorizationTests
    {
        public class TestEntity
        {
            public string AssessmentTitle { get; set; }
        }

        private static Claim Given_NamespacePrefix_claim_of(string namespacePrefix)
        {
            return new Claim(EdFiOdsApiClaimTypes.NamespacePrefix, namespacePrefix);
        }

        private static ClaimsPrincipal Given_ClaimsPrincipal_with_claims(params Claim[] claims)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(claims));
        }

        private static EdFiAuthorizationContext Given_authorization_context_with_data(object data)
        {
            return new EdFiAuthorizationContext(
                Given_ClaimsPrincipal_with_claims(),
                "resource", "action",
                data);
        }

        private static EdFiAuthorizationContext Given_authorization_context_with_namespace_claim_and_data(
            string namespacePrefix,
            object data)
        {
            Claim claim = Given_NamespacePrefix_claim_of(namespacePrefix);

            return new EdFiAuthorizationContext(
                Given_ClaimsPrincipal_with_claims(claim),
                "resource", "action",
                data);
        }

        // -------------------------------------------------------
        // NOTE: This is an exploratory style of unit testing.
        // -------------------------------------------------------
        public class When_authorizing_assessment_metadata_using_assessment_title
            : ScenarioFor<AssessmentMetadataAuthorizationStrategy>
        {
            protected override void Arrange()
            {
                Given<IAssessmentMetadataNamespaceProvider>()
                    .that_given_an_assessment_title_of("TEST_Title")
                    .returns("http://www.ed-fi.org/Something/Else/In/Path");

                Supplied(
                    Given_authorization_context_with_namespace_claim_and_data(
                        @"http://www.ed-fi.org/", 
                        new TestEntity {AssessmentTitle = "TEST_Title"}));
            }

            protected override void Act()
            {
                TestSubject.AuthorizeSingleItem(null, Supplied<EdFiAuthorizationContext>());
            }

            [Assert]
            public void Should_not_throw_an_exception()
            {
                ActualException.ShouldBeNull();
            }
        }

        public class When_authorizing_assessment_metadata_without_an_issued_NamespacePrefix_claim
            : ScenarioFor<AssessmentMetadataAuthorizationStrategy>
        {
            protected override void Arrange()
            {
                // Initialize dependencies
                Supplied(Given_authorization_context_with_data(new TestEntity()));
            }

            protected override void Act()
            {
                TestSubject.AuthorizeSingleItem(null, Supplied<EdFiAuthorizationContext>());
            }

            [Assert]
            public void Should_throw_an_EdFiSecurityException_indicating_access_could_not_be_authorized_due_to_missing_claim()
            {
                ActualException.ShouldBeExceptionType<EdFiSecurityException>();
                ActualException.MessageShouldContain("Access to the resource could not be authorized because the caller did not have a NamespacePrefix claim");
            }
        }
        // End exploratory style of testing

        [Test]
        [ExpectedException(ExpectedException = typeof(EdFiSecurityException), ExpectedMessage = "Access to the resource could not be authorized because the caller did not have a NamespacePrefix claim ('" + EdFiOdsApiClaimTypes.NamespacePrefix + "') or the claim value was empty.")]
        public void AssessmentMetadataAuthorization_EmptyNamespaceClaim()
        {
            //Arrange
            var repo = MockRepository.GenerateStub<IAssessmentMetadataNamespaceProvider>();
            repo.Stub(r => r.GetNamespaceByAssessmentTitle("TEST_Title")).Return("http://www.ed-fi.org/");

            var strategy = new AssessmentMetadataAuthorizationStrategy(repo);

            var claims = new List<Claim>
            {
                new Claim(EdFiOdsApiClaimTypes.NamespacePrefix, string.Empty)
            };

			ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(claims, EdFiAuthenticationTypes.OAuth));

            string resource = @"http://ed-fi.org/ods/identity/claims/assessment";
            string action = @"http://ed-fi.org/ods/actions/manage";

            var data = new AssessmentMetadataAuthorizationContextData { AssessmentTitle = @"TEST_Title" };

            //Act
            strategy.AuthorizeSingleItem(new List<Claim>(), new EdFiAuthorizationContext(principal, resource, action, data));

            //Assert
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(EdFiSecurityException), ExpectedMessage = "Access to the resource could not be authorized. The Namespace of the resource is empty.")]
        public void AssessmentMetadataAuthorization_EmptyResourceNamespace()
        {
            //Arrange
            var repo = MockRepository.GenerateStub<IAssessmentMetadataNamespaceProvider>();
            repo.Stub(r => r.GetNamespaceByAssessmentTitle("TEST_Title")).Return("http://www.ed-fi.org/");

            var strategy = new AssessmentMetadataAuthorizationStrategy(repo);

            var claims = new List<Claim>
            {
                new Claim(EdFiOdsApiClaimTypes.NamespacePrefix, @"http://www.ed-fi.org/")
            }; 
            
            ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(claims, EdFiAuthenticationTypes.OAuth));

            string resource = @"http://ed-fi.org/ods/identity/claims/assessment";
            string action = @"http://ed-fi.org/ods/actions/manage";

            var data = new AssessmentMetadataAuthorizationContextData { AssessmentTitle = string.Empty };

            //Act
            strategy.AuthorizeSingleItem(new List<Claim>(), new EdFiAuthorizationContext(principal, resource, action, data));

            //Assert
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(EdFiSecurityException), ExpectedMessage = "Access to the resource item with namespace 'http://www.TEST.org/' could not be authorized based on the caller's NamespacePrefix claim of 'http://www.ed-fi.org/'.")]
        public void AssessmentMetadataAuthorization_MismatchedNamespaces()
        {
            //Arrange
            var repo = MockRepository.GenerateStub<IAssessmentMetadataNamespaceProvider>();
            repo.Stub(r => r.GetNamespaceByAssessmentTitle("TEST_Title")).Return("http://www.TEST.org/");

            var strategy = new AssessmentMetadataAuthorizationStrategy(repo);

            var claims = new List<Claim>
            {
                new Claim(EdFiOdsApiClaimTypes.NamespacePrefix, @"http://www.ed-fi.org/")
            }; 
            
            ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(claims, EdFiAuthenticationTypes.OAuth));

            string resource = @"http://ed-fi.org/ods/identity/claims/assessment";
            string action = @"http://ed-fi.org/ods/actions/manage";

            var data = new AssessmentMetadataAuthorizationContextData { AssessmentTitle = @"TEST_Title" };

            //Act
            strategy.AuthorizeSingleItem(new List<Claim>(), new EdFiAuthorizationContext(principal, resource, action, data));

            //Assert
        }
    }
}
