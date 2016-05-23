using EdFi.Ods.Api.Common.Authorization;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Rhino.Mocks;
using Should;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace EdFi.Ods.WebService.Tests.ResponseTests
{
    [TestFixture]
    public class when_accessing_a_resource_with_invalid_token : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_when_accessing_a_resource_with_invalid_token_{0}", Guid.NewGuid().ToString("N"));
        
        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        protected IOAuthTokenValidator CreateOAuthTokenValidator()
        {
            var oAuthTokenValidator = MockRepository.GenerateStub<IOAuthTokenValidator>();
            oAuthTokenValidator.Stub(t => t.GetClientDetailsForToken(Arg<Guid>.Is.Anything)).Return(new ApiClientDetails
            {
                ApiKey = string.Empty,
                ApplicationId = int.MinValue,
                ClaimSetName = string.Empty,
                NamespacePrefix = string.Empty,
                EducationOrganizationIds = new List<int>(),
            });
            return oAuthTokenValidator;
        }

        [Test]
        public void api_should_fail_with_401_code()
        {
            using (var startup = new OwinStartup(DatabaseName, new List<int>(), string.Empty, null, CreateOAuthTokenValidator))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "Some Invalid Token");

                        var response = client.GetAsync(OwinUriHelper.BuildApiUri("2014", "courses", string.Empty)).Result;
                        response.IsSuccessStatusCode.ShouldBeFalse();
                        response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
                    }
                }
            }
        }
    }
}