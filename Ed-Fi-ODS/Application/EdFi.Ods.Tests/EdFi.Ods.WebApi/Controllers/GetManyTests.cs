namespace EdFi.Ods.Tests.EdFi.Ods.WebApi.Controllers
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    using global::EdFi.Ods.Api.Common;
    using global::EdFi.Ods.Api.Models.Resources.Student;
    using global::EdFi.Ods.Api.Services.Controllers.v2.Students;
    using global::EdFi.Ods.Pipelines.Factories;
    using global::EdFi.Ods.Tests._Bases;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using Should;

    public class StudentGetManyTests
    {
        [TestFixture]
        public class When_getting_students : TestBase
        {
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, new SingleStepPipelineProviderForTest(typeof (SimpleGetManyResourceCreationStep<,,,>)), null, null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._responseMessage = controller.GetAll(new UrlQueryParametersRequest()).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_student()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.OK);
                var result = this._responseMessage.Content.ReadAsStringAsync().Result;
                var students = JsonConvert.DeserializeObject<Student[]>(result);
                students.Length.ShouldEqual(25);
            }
        }

        [TestFixture]
        public class When_getting_students_with_upper_limit : TestBase
        {
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, new SingleStepPipelineProviderForTest(typeof(SimpleGetManyResourceCreationStep<,,,>)), null, null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._responseMessage = controller.GetAll(new UrlQueryParametersRequest { Limit = 10 }).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_student()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.OK);
                var result = this._responseMessage.Content.ReadAsStringAsync().Result;
                var students = JsonConvert.DeserializeObject<Student[]>(result);
                students.Length.ShouldEqual(10);
            }
        }

        [TestFixture]
        public class When_getting_students_with_upper_limit_past_max : TestBase
        {
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, new SingleStepPipelineProviderForTest(typeof(SimpleGetManyResourceCreationStep<,,,>)), null, null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._responseMessage = controller.GetAll(new UrlQueryParametersRequest{Limit = 1000}).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_student()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            }
        }


        [TestFixture]
        public class When_getting_students_unauthorized : TestBase
        {
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, new SingleStepPipelineProviderForTest(typeof (EdFiSecurityExceptionStep<,,,>)), null, null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._responseMessage = controller.GetAll(new UrlQueryParametersRequest()).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_forbidden()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
            }
        }


        [TestFixture]
        public class When_unhandled_exception_getting_students : TestBase
        {
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, null, new SingleStepPipelineProviderForTest(typeof(UnhandledExceptionStep<,,,>)), null, null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._responseMessage = controller.GetAll(new UrlQueryParametersRequest()).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_internal_server_error()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
            }
        }
    }
}
