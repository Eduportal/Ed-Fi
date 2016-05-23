using EdFi.Ods.Api.Services.ActionFilters;
using log4net;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Owin;
using Rhino.Mocks;
using Should;
using System.IdentityModel;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace EdFi.Ods.Tests.EdFi.Ods.WebApi
{
    public class ExceptionHandlingFilterTests
    {
        [TestFixture]
        public class When_uncaught_error_is_handled_by_error_filter_with_custom_errors
        {
            private class TestController : ApiController { }

            private HttpResponseMessage response;

            private const string ExpectedErrorMessage = @"{""Message"":""An error has occurred.""}";


            [TestFixtureSetUp]
            public void TestFixtureSetUp()
            {
                var httpControllerContext = new HttpControllerContext(new HttpRequestContext(), new HttpRequestMessage(), new HttpControllerDescriptor(), new TestController());
                var httpActionContext = new HttpActionContext(httpControllerContext, new ReflectedHttpActionDescriptor());
                var actionExecutedContext = new HttpActionExecutedContext(httpActionContext, new BadRequestException());

                var exceptionHandlingFilter = new ExceptionHandlingFilter(true);

                exceptionHandlingFilter.ExecuteExceptionFilterAsync(actionExecutedContext, new CancellationToken()).Wait();

                response = actionExecutedContext.Response;
            }

            [Test]
            public void Should_create_response_with_internal_server_error_status_code()
            {
                response.ShouldNotBeNull();
                response.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
            }

            [Test]
            public void generic_message_should_not_include_stack_trace()
            {
                var responseTask = response.Content.ReadAsStringAsync();
                responseTask.Wait();
                var result = responseTask.Result;
                result.ShouldEqual(ExpectedErrorMessage);
            }
        }
    }

    #region exception handling tests using Owin Test libraries

    public class UncaughtErrorTestController : ApiController
    {
        public IHttpActionResult Get()
        {
            throw new BadRequestException("Custom Message");
        }
    }

    public class OwinTests
    {

        private const string ErrorMessage = "{\"Message\":\"An error has occurred.\"}";

        [TestFixture]
        public class When_test_controller_throws_uncaught_error_and_custom_errors_is_on
        {
            public class Startup
            {
                public void Configuration(IAppBuilder appBuilder)
                {
                    var config = new HttpConfiguration();
                    config.Routes.MapHttpRoute("Default", "api/UncaughtErrorTest", new { controller = "UncaughtErrorTest", action = "Get" });
                    config.Filters.Add(new ExceptionHandlingFilter(true));
                    appBuilder.UseWebApi(config);
                }
            }

            private HttpResponseMessage result;

            [TestFixtureSetUp]
            public void TestFixtureSetUp()
            {
                using (var testServer = TestServer.Create<Startup>())
                {
                    var response = testServer.HttpClient.GetAsync("api/UncaughtErrorTest");
                    response.Wait();
                    result = response.Result;
                    testServer.Dispose();
                }
            }

            [Test]
            public void Should_have_error_status_code()
            {
                result.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
            }

            [Test]
            public void Should_be_generic_error_message()
            {
                var resultMessage = result.Content.ReadAsStringAsync().Result;
                resultMessage.ShouldEqual(ErrorMessage);
            }
        }

        [TestFixture]
        public class When_test_controller_throws_uncaught_error_and_custom_errors_is_off
        {
            public class Startup
            {
                public void Configuration(IAppBuilder appBuilder)
                {
                    var config = new HttpConfiguration();
                    config.Routes.MapHttpRoute("Default", "api/UncaughtErrorTest", new { controller = "UncaughtErrorTest", action = "Get" });
                    config.Filters.Add(new ExceptionHandlingFilter(false));
                    config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
                    appBuilder.UseWebApi(config);
                }
            }

            private HttpResponseMessage result;

            [TestFixtureSetUp]
            public void TestFixtureSetUp()
            {
                using (var testServer = TestServer.Create<Startup>())
                {
                    var response = testServer.HttpClient.GetAsync("api/UncaughtErrorTest");
                    response.Wait();
                    result = response.Result;
                    testServer.Dispose();
                }
            }

            [Test]
            public void Should_have_error_status_code()
            {
                result.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
            }

            [Test]
            public void Should_not_be_generic_error_message()
            {
                var resultMessage = result.Content.ReadAsStringAsync().Result;
                resultMessage.ShouldNotEqual(ErrorMessage);
            }
        }
    }

    #endregion exception handling tests using Owin Test libraries

}