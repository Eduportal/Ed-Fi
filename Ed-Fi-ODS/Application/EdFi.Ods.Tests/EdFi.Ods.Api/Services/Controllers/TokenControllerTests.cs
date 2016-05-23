using System;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.Api.Services.Controllers;
using EdFi.Ods.Tests._Bases;
using NUnit.Framework;
using Rhino.Mocks;

namespace EdFi.Ods.Tests.EdFi.Ods.Api.Services.Controllers
{
    public class TokenControllerTests
    {
        [TestFixture]
        public class When_calling_the_token_controller : TestFixtureBase
        {
            private TokenController _controller;

            protected override void Arrange()
            {
                _controller = new TokenController();
                var config = new HttpConfiguration();
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/token");
                var route = config.Routes.MapHttpRoute("default", "api/{controller}/{id}");
                var routeData = new HttpRouteData(route, new HttpRouteValueDictionary {{"controller", "authorize"}});

                _controller.ControllerContext = new HttpControllerContext(config, routeData, request);
                _controller.Request = request;
                _controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            }

            [Assert]
            public void Good_request_should_add_return_client_access_token()
            {
                var goodAuthCode = Guid.NewGuid();
                var goodClient = new ApiClient(){};
                var mock = MockRepository.GenerateMock<IClientAppRepo>();
                mock.Expect(x => x.GetClient(Arg<string>.Is.Equal("good_clientId"), Arg<String>.Is.Equal("good_clientSecret")))
                    .Return(goodClient);
                mock.Expect(x => x.GetClientAuthorizationCode(Arg<Guid>.Is.Anything))
                    .Return(new ClientAuthorizationCode(){ApiClient = goodClient, Id = goodAuthCode});
                mock.Expect(x => x.AddClientAccessToken(Arg<ClientAuthorizationCode>.Is.Anything))
                    .Return(new ClientAccessToken() {ApiClient = goodClient, Id = goodAuthCode});

                mock.AssertWasNotCalled(x => x.GetClient(Arg<string>.Is.Anything));
                mock.AssertWasNotCalled(x=> x.GetClient(Arg<string>.Is.Anything, Arg<string>.Is.Null));
                mock.AssertWasNotCalled(x => x.GetClient(Arg<string>.Is.Anything, Arg<string>.Is.Equal(string.Empty)));

                _controller.ClientAppRepo = mock;

                var result = _controller.Post(new TokenRequest()
                {
                    Client_id = "good_clientId",
                    Client_secret = "good_clientSecret",
                    Code = goodAuthCode.ToString("N"),
                    Grant_type = "authorization_code"
                }).ExecuteAsync(new CancellationToken()).Result;

                mock.VerifyAllExpectations();
            }
        }
    }
}
