using Castle.Windsor;
namespace EdFi.Ods.Tests.EdFi.Ods.WebApi.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web.Http;

    using global::EdFi.Common.InversionOfControl;
    using global::EdFi.Ods.Api.Common;
    using global::EdFi.Ods.Api.Models.Requests.v2.Students;
    using global::EdFi.Ods.Api.Services.Controllers.v2.Students;
    using global::EdFi.Ods.Api.Services.Extensions;
    using global::EdFi.Ods.Common.Context;
    using global::EdFi.Ods.Pipelines.Factories;
    using global::EdFi.Ods.Tests._Bases;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using Should;

    public class StudentPostTests
    {
        [TestFixture]
        public class When_posting_a_non_existing_student : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;
            private IServiceLocator _locator;

            [TestFixtureSetUp]
            public void Setup()
            {
                _locator = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory = 
                    new PipelineFactory(_locator, null, null, null, new SingleStepPipelineProviderForTest(typeof(PersistNewModel<,,,>)), null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory, this._id.ToString("N"));
                this._id = Guid.NewGuid();
                this._responseMessage = controller.Post(new StudentPost { ETag = this._id.ToString("n") }).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_created_result()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.Created);
            }

            [Test]
            public void Should_contain_location_header_in_response()
            {
                var schoolYearProvider = _locator.Resolve<ISchoolYearContextProvider>();
                this._responseMessage.Headers.Location.AbsoluteUri.ShouldEqual(
                    String.Format("http://localhost/api/v2.0/{0}/students/{1}", schoolYearProvider.GetSchoolYear(), this._id.ToString("n")));
            }

            [Test]
            public void Should_contain_updated_etag_in_response()
            {
                var etagprovider = _locator.Resolve<IETagProvider>();
                this._responseMessage.Headers.ETag.ShouldEqual(new EntityTagHeaderValue(etagprovider.GetETag(this._id.ToString("n")).Quoted()));
            }
        }

        [TestFixture]
        public class When_posting_an_existing_student : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;
            private IServiceLocator _container;

            [TestFixtureSetUp]
            public void Setup()
            {
                _container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(_container, null, null, null, new SingleStepPipelineProviderForTest(typeof(PersistExistingModel<,,,>)), null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory, this._id.ToString("N"));
                this._id = Guid.NewGuid();
                this._responseMessage = controller.Post(new StudentPost { ETag = this._id.ToString("n") }).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_ok()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.OK);
            }
            
            [Test]
            public void Should_contain_location_header_in_response()
            {
                var schoolYearProvider = _container.Resolve<ISchoolYearContextProvider>();
                this._responseMessage.Headers.Location.AbsoluteUri.ShouldEqual(
                    String.Format("http://localhost/api/v2.0/{0}/students/{1}", schoolYearProvider.GetSchoolYear(), this._id.ToString("n")));
            }

            [Test]
            public void Should_contain_updated_etag_in_response()
            {
                var etagprovider = _container.Resolve<IETagProvider>();
                this._responseMessage.Headers.ETag.ShouldEqual(new EntityTagHeaderValue(etagprovider.GetETag(this._id.ToString("n")).Quoted()));
            }
        }
        
        [TestFixture]
        public class When_posting_a_student_with_id_set : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, null, new SingleStepPipelineProviderForTest(typeof(PersistNewModel<,,,>)), null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory, this._id.ToString("N"));
                this._id = Guid.NewGuid();
                this._responseMessage = controller.Post(new StudentPost { ETag = "etag", Id = this._id}).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_bad_request()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            }

            [Test]
            public void Should_return_message()
            {
                var result = this._responseMessage.Content.ReadAsStringAsync().Result;
                var resource = JsonConvert.DeserializeObject<HttpError>(result);
                resource["Message"].ShouldEqual("Resource identifiers cannot be assigned by the client.");
            }
        }

        [TestFixture]
        public class When_posting_a_student_causes_unique_id_exception : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, null, new SingleStepPipelineProviderForTest(typeof(InsertUniqueIdExceptionStep<,,,>)), null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory, this._id.ToString("N"));
                this._id = Guid.NewGuid();
                this._responseMessage = controller.Post(new StudentPost()).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_conflict()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.Conflict);
            }

            [Test]
            public void Should_return_message()
            {
                var result = this._responseMessage.Content.ReadAsStringAsync().Result;
                var resource = JsonConvert.DeserializeObject<HttpError>(result);
                var expression = new Regex(@"^The value supplied for property '(?<ForeignKeyName>\w+)' of entity '(?<TableName>\w+)' is not unique.");
                expression.Match(resource.Message).Success.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_posting_a_student_causes_referencial_exception : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, null, new SingleStepPipelineProviderForTest(typeof(InsertReferentialExceptionStep<,,,>)), null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory, this._id.ToString("N"));
                this._id = Guid.NewGuid();
                this._responseMessage = controller.Post(new StudentPost()).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_conflict()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.Conflict);
            }

            [Test]
            public void Should_return_message()
            {
                var result = this._responseMessage.Content.ReadAsStringAsync().Result;
                var resource = JsonConvert.DeserializeObject<HttpError>(result);
                var expression = new Regex(@"^The value supplied for the related '(?<ConstraintName>\w+)' resource does not exist.");
                expression.Match(resource.Message).Success.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_posting_a_student_causes_validation_exception : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, null, new SingleStepPipelineProviderForTest(typeof(ValidationExceptionStep<,,,>)), null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory, this._id.ToString("N"));
                this._id = Guid.NewGuid();
                this._responseMessage = controller.Post(new StudentPost()).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_bad_request()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            }

            [Test]
            public void Should_return_message()
            {
                var result = this._responseMessage.Content.ReadAsStringAsync().Result;
                var resource = JsonConvert.DeserializeObject<HttpError>(result);
                resource["Message"].ShouldEqual("Exception for testing");
            }
        }

        [TestFixture]
        public class When_posting_student_unauthorized : TestBase
        {
            [Test]
            public void Should_return_forbidden()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var id = Guid.NewGuid();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, null, new SingleStepPipelineProviderForTest(typeof(EdFiSecurityExceptionStep<,,,>)), null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory, id.ToString("N"));
                var responseMessage = controller.Post(new StudentPost()).ExecuteAsync(new CancellationToken()).Result;
                responseMessage.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
            }
        }

        [TestFixture]
        public class When_posting_student_throws_unhandled_exception : TestBase
        {
            //private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var id = Guid.NewGuid();
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, null, new SingleStepPipelineProviderForTest(typeof(UnhandledExceptionStep<,,,>)), null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory, id.ToString("N"));
                this._responseMessage = controller.Post(new StudentPost()).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_internal_server_error()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
            }

            [Test]
            public void Should_return_message()
            {
                var result = this._responseMessage.Content.ReadAsStringAsync().Result;
                var resource = JsonConvert.DeserializeObject<HttpError>(result);
                resource.Message.ShouldEqual("An unexpected error occurred on the server.");
            }
        }
    }
}
