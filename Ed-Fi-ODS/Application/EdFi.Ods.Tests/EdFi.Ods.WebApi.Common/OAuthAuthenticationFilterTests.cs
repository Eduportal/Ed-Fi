using EdFi.Common.Security.Claims;
using EdFi.Ods.Api.Common.Authorization;
using EdFi.Ods.Api.Common.Filters;
using EdFi.Ods.Common.Security;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace EdFi.Ods.Tests.EdFi.Ods.WebApi.Common
{
    [TestFixture]
    public class OAuthAuthenticationFilterTests
    {
        private HttpAuthenticationContext GetBaseAuthenticationContext()
        {
            var request = new HttpRequestMessage();
            var controllerContext = new HttpControllerContext
            {
                Request = request,
            };

            var actionContext = new HttpActionContext
            {
                ControllerContext = controllerContext,
            };

            return new HttpAuthenticationContext(actionContext, null);
            
        }

        private void SetAuthenticationHeader(HttpAuthenticationContext context, string scheme, string parameter)
        {
            var authorization = new AuthenticationHeaderValue(scheme, parameter);
            var headers = context.Request.Headers;
            headers.Authorization = authorization;
        }

        [Test]
        public void OAuthAuthenticationFilter_NoCredentials()
        {
            //arrange
            var tokenValidator = MockRepository.GenerateStub<IOAuthTokenValidator>();
            var contextProvider = MockRepository.GenerateStub<IApiKeyContextProvider>();
            var identityProvider = MockRepository.GenerateStub<IClaimsIdentityProvider>();

            var filter = new OAuthAuthenticationFilter(tokenValidator, contextProvider, identityProvider);

            var context = GetBaseAuthenticationContext();
            var cancellationToken = new CancellationToken();
            
            //act
            filter.AuthenticateAsync(context, cancellationToken);

            //assert
            Assert.IsNull(context.Principal, "Principal should not be set if no Authentication headers are present.");
            Assert.IsNull(context.ErrorResult, "No errors should be set if no Authentication headers are present.");
        }

        [Test]
        public void OAuthAuthenticationFilter_InvalidScheme()
        {
            //arrange
            var tokenValidator = MockRepository.GenerateStub<IOAuthTokenValidator>();
            var contextProvider = MockRepository.GenerateStub<IApiKeyContextProvider>();
            var identityProvider = MockRepository.GenerateStub<IClaimsIdentityProvider>();

            var filter = new OAuthAuthenticationFilter(tokenValidator, contextProvider, identityProvider);

            var context = GetBaseAuthenticationContext();
            var cancellationToken = new CancellationToken();

            SetAuthenticationHeader(context, "testScheme", null);

            //act
            filter.AuthenticateAsync(context, cancellationToken);

            //assert
            Assert.IsNull(context.Principal, "Principal should not be set if Authentication scheme does not match supported scheme.");
            Assert.IsNull(context.ErrorResult, "No errors should be set if Authentication scheme does not match supported scheme.");
        }

        [Test]
        public void OAuthAuthenticationFilter_MissingParameter()
        {
            //arrange
            var tokenValidator = MockRepository.GenerateStub<IOAuthTokenValidator>();
            var contextProvider = MockRepository.GenerateStub<IApiKeyContextProvider>();
            var identityProvider = MockRepository.GenerateStub<IClaimsIdentityProvider>();

            var filter = new OAuthAuthenticationFilter(tokenValidator, contextProvider, identityProvider);

            var context = GetBaseAuthenticationContext();
            var cancellationToken = new CancellationToken();

            SetAuthenticationHeader(context, "Bearer", null);

            //act
            filter.AuthenticateAsync(context, cancellationToken);

            //assert
            Assert.IsNull(context.Principal, "Principal should not be set when no credentials supplied.");
            Assert.IsNotNull(context.ErrorResult, "Should get an error when no credentials supplied.");
            Assert.AreEqual(context.ErrorResult.ExecuteAsync(cancellationToken).Result.ReasonPhrase, "Missing credentials");
        }

        [Test]
        public void OAuthAuthenticationFilter_InvalidParameter()
        {
            //arrange
            var tokenValidator = MockRepository.GenerateStub<IOAuthTokenValidator>();
            var contextProvider = MockRepository.GenerateStub<IApiKeyContextProvider>();
            var identityProvider = MockRepository.GenerateStub<IClaimsIdentityProvider>();

            var filter = new OAuthAuthenticationFilter(tokenValidator, contextProvider, identityProvider);

            var context = GetBaseAuthenticationContext();
            var cancellationToken = new CancellationToken();

            SetAuthenticationHeader(context, "Bearer", "notaguid");

            //act
            filter.AuthenticateAsync(context, cancellationToken);

            //assert
            Assert.IsNull(context.Principal, "Principal should not be set when invalid credentials supplied.");
            Assert.IsNotNull(context.ErrorResult, "Should get an error when invalid credentials supplied.");
            Assert.AreEqual(context.ErrorResult.ExecuteAsync(cancellationToken).Result.ReasonPhrase, "Invalid credentials");
        }

        [Test]
        public void OAuthAuthenticationFilter_InvalidOauthToken()
        {
            //arrange
            var tokenValidator = MockRepository.GenerateStub<IOAuthTokenValidator>();
            tokenValidator.Stub(t => t.GetClientDetailsForToken(Arg<Guid>.Is.Anything)).Return(new ApiClientDetails());

            var contextProvider = MockRepository.GenerateStub<IApiKeyContextProvider>();
            var identityProvider = MockRepository.GenerateStub<IClaimsIdentityProvider>();

            var filter = new OAuthAuthenticationFilter(tokenValidator, contextProvider, identityProvider);

            var context = GetBaseAuthenticationContext();
            var cancellationToken = new CancellationToken();

            SetAuthenticationHeader(context, "Bearer", Guid.NewGuid().ToString());

            //act
            filter.AuthenticateAsync(context, cancellationToken);

            //assert
            Assert.IsNull(context.Principal, "Principal should not be set when an invalid token is supplied.");
            Assert.IsNotNull(context.ErrorResult, "Should get an error when an invalid token is supplied.");
            Assert.AreEqual(context.ErrorResult.ExecuteAsync(cancellationToken).Result.ReasonPhrase, "Invalid token");
        }

        [Test]
        public void OAuthAuthenticationFilter_ValidCredentials()
        {
            //arrange
            var setContextCalled = false;

            var tokenValidator = MockRepository.GenerateStub<IOAuthTokenValidator>();
            tokenValidator.Stub(t => t.GetClientDetailsForToken(Arg<Guid>.Is.Anything)).Return(new ApiClientDetails { ApiKey = "key", ClaimSetName = "claimSetName", NamespacePrefix = "namespacePrefix", EducationOrganizationIds = new List<int>() });

            var contextProvider = MockRepository.GenerateStub<IApiKeyContextProvider>();
            contextProvider.Stub(c => c.SetApiKeyContext(Arg<ApiKeyContext>.Is.Anything)).WhenCalled(call => { setContextCalled = true; });

            var identityProvider = MockRepository.GenerateStub<IClaimsIdentityProvider>();
            identityProvider.Stub(i => i.GetClaimsIdentity(Arg<IEnumerable<int>>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(new ClaimsIdentity());

            var filter = new OAuthAuthenticationFilter(tokenValidator, contextProvider, identityProvider);

            var context = GetBaseAuthenticationContext();
            var cancellationToken = new CancellationToken();

            SetAuthenticationHeader(context, "Bearer", Guid.NewGuid().ToString());

            //act
            filter.AuthenticateAsync(context, cancellationToken);

            //assert
            Assert.IsNull(context.ErrorResult, "Should not get an error when an valid credentials are supplied.");
            Assert.IsNotNull(context.Principal, "Principal should be set when an valid credentials are supplied.");

            Assert.IsTrue(setContextCalled);
            
        }
    }
}
