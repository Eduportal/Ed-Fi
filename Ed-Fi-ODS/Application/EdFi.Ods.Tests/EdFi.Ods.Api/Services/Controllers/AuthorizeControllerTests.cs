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
    public class AuthorizeControllerTests
    {
        [TestFixture]
        public class When_calling_the_authorize_controller : TestFixtureBase
        {
            private AuthorizeController _controller;

            protected override void Arrange()
            {
                _controller = new AuthorizeController();
                var config = new HttpConfiguration();
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/authorize");
                var route = config.Routes.MapHttpRoute("default", "api/{controller}/{id}");
                var routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "authorize" } });

                _controller.ControllerContext = new HttpControllerContext(config, routeData, request);
                _controller.Request = request;
                _controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            }

            [Assert]
            public void Good_request_should_add_client_authorization_code()
            {
                var goodClient = new ApiClient();
                var mock = MockRepository.GenerateMock<IClientAppRepo>();
                mock.Expect(x => x.GetClient(Arg<string>.Is.Equal("good")))
                    .Return(goodClient);
                mock.Expect(x => x.AddClientAuthorizationCode(Arg<int>.Is.Anything, Arg<string>.Is.Anything))
                    .Return(new ClientAuthorizationCode());

                mock.AssertWasNotCalled(x => x.GetClient(Arg<string>.Is.Anything, Arg<string>.Is.Anything));

                _controller.ClientAppRepo = mock;

                var result = _controller.Post(new AuthRequest()
                {
                    Client_id = "good",
                    Response_type = "code"
                }).ExecuteAsync(new CancellationToken()).Result;

                mock.VerifyAllExpectations();
            }

            [Assert]
            public void Bad_request_should_return_invalid_authorization_token()
            {
                var mock = MockRepository.GenerateMock<IClientAppRepo>();
                mock.Expect(x => x.GetClient(Arg<string>.Is.NotEqual("good")))
                    .Return(null);

                mock.AssertWasNotCalled(x => x.GetClient(Arg<string>.Is.Anything, Arg<string>.Is.Anything));
                mock.AssertWasNotCalled(x => x.AddClientAuthorizationCode(Arg<int>.Is.Anything, Arg<string>.Is.Anything));

                _controller.ClientAppRepo = mock;

                var result = _controller.Post(new AuthRequest(){
                    Client_id = "bad",
                    Response_type = "code"
                }).ExecuteAsync(new CancellationToken()).Result;

                var strResult = result.Content.ReadAsStringAsync().Result;
                Assert.IsFalse(strResult.Contains("invalid_request"));

                mock.VerifyAllExpectations();
            }
        }
    }
}
