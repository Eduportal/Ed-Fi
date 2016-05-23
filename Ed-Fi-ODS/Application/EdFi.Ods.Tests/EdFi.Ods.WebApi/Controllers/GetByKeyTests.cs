using EdFi.Ods.Api.Services.Controllers.v2.AcademicWeeks;

namespace EdFi.Ods.Tests.EdFi.Ods.WebApi.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    using global::EdFi.Ods.Api.Models.Resources.Student;
    using global::EdFi.Ods.Api.Services.Controllers.v2.Students;
    using global::EdFi.Ods.Pipelines.Factories;
    using global::EdFi.Ods.Tests._Bases;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using Should;

    public class StudentGetByKeyTests
    {
        [TestFixture]
        public class When_getting_student_by_key : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, new SingleStepPipelineProviderForTest(typeof(SimpleGetByKeyResourceCreationStep<,,,>)), null, null, null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._id = Guid.NewGuid();
               this._responseMessage = controller.GetByKey(this._id.ToString()).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_student()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.OK);
                var result = this._responseMessage.Content.ReadAsStringAsync().Result;
                var resource = JsonConvert.DeserializeObject<Student>(result);
                resource.StudentUniqueId.ShouldEqual(this._id.ToString());
            }
        }

        [TestFixture]
        public class When_getting_student_by_key_not_found : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, new SingleStepPipelineProviderForTest(typeof(SetNullResourceStep<,,,>)), null, null, null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._id = Guid.NewGuid();
                this._responseMessage = controller.GetByKey(this._id.ToString()).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_not_found()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
            }
        }

        [TestFixture]
        public class When_getting_student_by_key_unauthorized : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, new SingleStepPipelineProviderForTest(typeof(EdFiSecurityExceptionStep<,,,>)), null, null, null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._id = Guid.NewGuid();
                this._responseMessage = controller.GetByKey(this._id.ToString()).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_forbidden()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);
            }
        }

        [TestFixture]
        public class When_unhandled_exception_getting_student_by_key : TestBase
        {
            private Guid _id;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null, new SingleStepPipelineProviderForTest(typeof(UnhandledExceptionStep<,,,>)), null, null, null);
                var controller = TestControllerBuilder.GetController<StudentsController>(pipelineFactory);
                this._id = Guid.NewGuid();
                this._responseMessage = controller.GetByKey(this._id.ToString()).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_internal_server_error()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
            }
        }
    }

    public class AcademicWeekGetByKeyTests
    {
        [TestFixture]
        public class When_getting_academic_week_by_key_not_found : TestBase
        {
            private string _weekIdentifier;
            private int _schoolId;
            private HttpResponseMessage _responseMessage;

            [TestFixtureSetUp]
            public void Setup()
            {
                var container = TestControllerBuilder.GetWindsorContainer();
                var pipelineFactory =
                    new PipelineFactory(container, null,
                        new SingleStepPipelineProviderForTest(typeof (SetNullResourceStep<,,,>)), null, null, null);
                var controller = TestControllerBuilder.GetController<AcademicWeeksController>(pipelineFactory);
                _weekIdentifier = "23";
                _schoolId = 1;
                this._responseMessage =
                    controller.GetByKey(_weekIdentifier, _schoolId).ExecuteAsync(new CancellationToken()).Result;
            }

            [Test]
            public void Should_return_not_found()
            {
                this._responseMessage.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
            }
        }
    }
}