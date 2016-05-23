namespace EdFi.Ods.Tests.EdFi.Ods.WebApi.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Web.Http;

    using global::EdFi.Ods.Api.Services.Controllers.v2.Students;
    using global::EdFi.Ods.Api.Services.Extensions;
    using global::EdFi.Ods.Pipelines.Factories;
    using global::EdFi.Ods.Tests._Bases;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using Should;

    class StudentDeleteTests
    {
        [TestFixture]
        public class When_deleting_existing_student : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, null, null, new SingleStepPipelineProviderForTest(typeof(EmptyResourceStep<,,,>)));
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._id = Guid.NewGuid();
                this._responseMessage = controller.Delete(this._id).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_no_content()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
            }
            [Test]
            public void Should_return_empty_content()
            {
                this._responseMessage.Content.ShouldBeNull();
            }
        }

        [TestFixture]
        public class When_deleting_student_not_found : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, null, null, new SingleStepPipelineProviderForTest(typeof(NotFoundExceptionStep<,,,>)));
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._id = Guid.NewGuid();
                this._responseMessage = controller.Delete(this._id).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_not_found()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
            }
        }

        [TestFixture]
        public class When_deleting_a_student_causes_referencial_exception : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, null, null, new SingleStepPipelineProviderForTest(typeof(DeleteReferentialExceptionStep<,,,>)));
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory, this._id.ToString("N"));
                this._id = Guid.NewGuid();
                this._responseMessage = controller.Delete(this._id).ExecuteAsync(new CancellationToken()).Result;
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
                resource.Message.ShouldContain("The resource (or a subordinate entity of the resource) cannot be deleted because it is a dependency");
            }
        }

        [TestFixture]
        public class When_deleting_student_unauthorized : TestBase
        {
            [Test]
            public void Should_return_forbidden()
            {
                var id = Guid.NewGuid();
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, null, null, new SingleStepPipelineProviderForTest(typeof(EdFiSecurityExceptionStep<,,,>)));
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory, id.ToString("N"));
                var responseMessage = controller.Delete(id).ExecuteAsync(new CancellationToken()).Result;
                responseMessage.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
            }
        }

        [TestFixture]
        public class When_deleting_student_throws_unhandled_exception : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                this._id = Guid.NewGuid();
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, null, null, new SingleStepPipelineProviderForTest(typeof(UnhandledExceptionStep<,,,>)));
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory, this._id.ToString("N"));
                this._responseMessage = controller.Delete(this._id).ExecuteAsync(new CancellationToken()).Result;
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

        [TestFixture]
        public class When_deleting_a_student_that_has_been_modified : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, null, null, new SingleStepPipelineProviderForTest(typeof(ConcurrencyExceptionStep<,,,>)));
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory, this._id.ToString("N"));
                this._id = Guid.NewGuid();
                controller.Request.Headers.IfMatch.Add(new EntityTagHeaderValue(this._id.ToString("N").Quoted()));
                this._responseMessage = controller.Delete(this._id).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_precondition_failed()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.PreconditionFailed);
            }

            [Test]
            public void Should_return_message()
            {
                var result = this._responseMessage.Content.ReadAsStringAsync().Result;
                var resource = JsonConvert.DeserializeObject<HttpError>(result);
                resource.Message.ShouldEqual("Resource was modified by another consumer.");
            }
        }
    }
}
