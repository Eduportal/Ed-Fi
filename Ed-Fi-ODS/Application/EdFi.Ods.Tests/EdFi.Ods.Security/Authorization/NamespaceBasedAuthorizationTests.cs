using System.Collections.Generic;
using System.Security.Claims;
using EdFi.Common.Configuration;
using EdFi.Common.Security;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Security.AuthorizationStrategies.NamespaceBased;
using EdFi.Ods.Security.Claims;
using NUnit.Framework;
using Rhino.Mocks;

namespace EdFi.Ods.Tests.EdFi.Ods.Security.Authorization
{
    [TestFixture]
    public class NamespaceBasedAuthorizationStrategyTests
    {
        [Test]
        public void NamespaceBasedAuthorization_MatchOnNamespace_ShouldThrowNoExceptions()
        {
            //Arrange
            var strategy = new NamespaceBasedAuthorizationStrategy(MockRepository.GenerateStub<IConfigValueProvider>());

            var claims = new List<Claim>
            {
                new Claim(EdFiOdsApiClaimTypes.NamespacePrefix, @"http://www.ed-fi.org/")
            };

            ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(claims, EdFiAuthenticationTypes.OAuth));

            string resource = @"http://ed-fi.org/ods/identity/claims/academicSubjectDescriptor";
            string action = @"http://ed-fi.org/ods/actions/manage";

            var data = new NamespaceBasedAuthorizationContextData { Namespace = @"http://www.ed-fi.org/" };

            //Act
            strategy.AuthorizeSingleItem(new List<Claim>(), new EdFiAuthorizationContext(principal, resource, action, data));

            //Assert
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(EdFiSecurityException), ExpectedMessage = "Access to the resource could not be authorized because the caller did not have a NamespacePrefix claim ('" + EdFiOdsApiClaimTypes.NamespacePrefix + "') or the claim value was empty.")]
        public void NamespaceBasedAuthorization_NoNamespaceClaim()
        {
            //Arrange
            var strategy = new NamespaceBasedAuthorizationStrategy(MockRepository.GenerateStub<IConfigValueProvider>());

            var claims = new List<Claim>();
            ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(claims, EdFiAuthenticationTypes.OAuth));

            string resource = @"http://ed-fi.org/ods/identity/claims/academicSubjectDescriptor";
            string action = @"http://ed-fi.org/ods/actions/manage";

            var data = new NamespaceBasedAuthorizationContextData { Namespace = @"http://www.ed-fi.org/" };

            //Act
            strategy.AuthorizeSingleItem(new List<Claim>(), new EdFiAuthorizationContext(principal, resource, action, data));

            //Assert
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(EdFiSecurityException), ExpectedMessage = "Access to the resource could not be authorized because the caller did not have a NamespacePrefix claim ('" + EdFiOdsApiClaimTypes.NamespacePrefix + "') or the claim value was empty.")]
        public void NamespaceBasedAuthorization_EmptyNamespaceClaim()
        {
            //Arrange
            var strategy = new NamespaceBasedAuthorizationStrategy(MockRepository.GenerateStub<IConfigValueProvider>());

            var claims = new List<Claim>
            {
                new Claim(EdFiOdsApiClaimTypes.NamespacePrefix, string.Empty)
            };

            ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(claims, EdFiAuthenticationTypes.OAuth));

            string resource = @"http://ed-fi.org/ods/identity/claims/academicSubjectDescriptor";
            string action = @"http://ed-fi.org/ods/actions/manage";

            var data = new NamespaceBasedAuthorizationContextData { Namespace = @"http://www.ed-fi.org/" };

            //Act
            strategy.AuthorizeSingleItem(new List<Claim>(), new EdFiAuthorizationContext(principal, resource, action, data));

            //Assert
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(EdFiSecurityException), ExpectedMessage = "Access to the resource item could not be authorized because the Namespace of the resource is empty.")]
        public void NamespaceBasedAuthorization_EmptyResourceNamespace()
        {
            //Arrange
            var strategy = new NamespaceBasedAuthorizationStrategy(MockRepository.GenerateStub<IConfigValueProvider>());

            var claims = new List<Claim>
            {
                new Claim(EdFiOdsApiClaimTypes.NamespacePrefix, @"http://www.ed-fi.org/")
            };

            ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(claims, EdFiAuthenticationTypes.OAuth));

            string resource = @"http://ed-fi.org/ods/identity/claims/academicSubjectDescriptor";
            string action = @"http://ed-fi.org/ods/actions/manage";

            var data = new NamespaceBasedAuthorizationContextData { Namespace = @"" };

            //Act
            strategy.AuthorizeSingleItem(new List<Claim>(), new EdFiAuthorizationContext(principal, resource, action, data));

            //Assert
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(EdFiSecurityException), ExpectedMessage = "Access to the resource item with namespace 'http://www.TEST.org/' could not be authorized based on the caller's NamespacePrefix claim of 'http://www.ed-fi.org/'.")]
        public void NamespaceBasedAuthorization_MismatchedNamespaces()
        {
            //Arrange
            var strategy = new NamespaceBasedAuthorizationStrategy(MockRepository.GenerateStub<IConfigValueProvider>());

            var claims = new List<Claim>
            {
                new Claim(EdFiOdsApiClaimTypes.NamespacePrefix, @"http://www.ed-fi.org/")
            };

            ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(claims, EdFiAuthenticationTypes.OAuth));

            string resource = @"http://ed-fi.org/ods/identity/claims/academicSubjectDescriptor";
            string action = @"http://ed-fi.org/ods/actions/manage";

            var data = new NamespaceBasedAuthorizationContextData { Namespace = @"http://www.TEST.org/" };

            //Act
            strategy.AuthorizeSingleItem(new List<Claim>(), new EdFiAuthorizationContext(principal, resource, action, data));

            //Assert
        }
    }
}
