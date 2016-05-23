namespace EdFi.Ods.Tests.EdFi.Ods.WebApi.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Web.Http;

    using global::EdFi.Ods.Api.Models.Resources.Student;
    using global::EdFi.Ods.Api.Services.Controllers.v2.Students;
    using global::EdFi.Ods.Pipelines.Factories;
    using global::EdFi.Ods.Tests._Bases;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using Should;

    public class StudentGetByIdTests
    {
        [TestFixture]
        public class When_getting_student_by_id : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, new SingleStepPipelineProviderForTest(typeof(SimpleResourceCreationStep<,,,>)), null, null, null, null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._id = Guid.NewGuid();
                this._responseMessage = controller.Get(this._id).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_student()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.OK);
                var result = this._responseMessage.Content.ReadAsStringAsync().Result;
                var resource = JsonConvert.DeserializeObject<Student>(result);
                resource.Id.ShouldEqual(this._id);
            }
        }

        [TestFixture]
        public class When_getting_student_by_id_not_found : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, new SingleStepPipelineProviderForTest(typeof (NotFoundExceptionStep<,,,>)), null, null, null, null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._id = Guid.NewGuid();
                this._responseMessage = controller.Get(this._id).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_not_found()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.NotFound);       
            }

            [Test]
            public void Should_contain_error_message_in_content()
            {
                var result = this._responseMessage.Content.ReadAsStringAsync().Result;
                var resource = JsonConvert.DeserializeObject<HttpError>(result);
                resource.Message.ShouldEqual("Exception for testing");
            }
        }

        [TestFixture]
        public class When_getting_student_by_id_not_modified : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, new SingleStepPipelineProviderForTest(typeof(NotModifiedExceptionStep<,,,>)), null, null, null, null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._id = Guid.NewGuid();
                this._responseMessage = controller.Get(this._id).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_not_modified()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.NotModified);
            }
        }

        [TestFixture]
        public class When_getting_student_by_id_unauthorized : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, new SingleStepPipelineProviderForTest(typeof(EdFiSecurityExceptionStep<,,,>)), null, null, null, null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._id = Guid.NewGuid();
                this._responseMessage = controller.Get(this._id).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_forbidden()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
            }
        }


        [TestFixture]
        public class When_unhandled_exception_getting_student_by_id : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, new SingleStepPipelineProviderForTest(typeof(UnhandledExceptionStep<,,,>)), null, null, null, null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._id = Guid.NewGuid();
                this._responseMessage = controller.Get(this._id).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_internal_server_error()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
            }
        }
    }
}
