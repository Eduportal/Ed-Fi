namespace EdFi.Ods.Tests.EdFi.Ods.WebApi
{
    using System;
    using System.Web.Http.Results;

    using global::EdFi.Ods.Api.Data.Contexts;
    using global::EdFi.Ods.Api.Data.Repositories.BulkOperations;
    using global::EdFi.Ods.Api.Models.Resources;
    using global::EdFi.Ods.Api.Services.Controllers;
    using global::EdFi.Ods.Common;
    using global::EdFi.Ods.Common.Context;
    using global::EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    [TestFixture]
    public class When_calling_get_by_id_of_bulk_operations_controller : TestBase
    {
        [Test]
        public void Should_return_bad_request_when_id_is_not_provided()
        {
            var sut = new BulkOperationsController(null, null, null);
            sut.Get(null).ShouldBeType<BadRequestResult>();
        }

        [Test]
        public void Should_return_not_found_when_id_is_not_valid()
        {
            var mockCreateBulkOperationAndGetById = MockRepository.GenerateMock<ICreateBulkOperationAndGetById>();

            mockCreateBulkOperationAndGetById.Expect(x => x.GetByIdAndYear((Arg<string>.Is.Anything), (Arg<int>.Is.Anything))).IgnoreArguments().Return(null);
            var mockSchoolYearProvider = this.Stub<ISchoolYearContextProvider>();
            mockSchoolYearProvider.Stub(x => x.GetSchoolYear()).Return(DateTime.Now.Year);

            var sut = new BulkOperationsController(mockCreateBulkOperationAndGetById, null, mockSchoolYearProvider);

            sut.Get("Invalid Id").ShouldBeType<NotFoundResult>();

            mockCreateBulkOperationAndGetById.AssertWasCalled(x => x.GetByIdAndYear(Arg<string>.Is.Anything, (Arg<int>.Is.Anything)));
        }

        [Test]
        public void Should_return_not_found_when_year_does_not_match_id()
        {
            var operationId = Guid.NewGuid();

            var mockCtx = MockRepository.GenerateMock<IBulkOperationDbContext>();
            var executor = new DbExecutor<IBulkOperationDbContext>(() => mockCtx);
            var mockCreateBulkOperationAndGetById = new CreateBulkOperationAndGetById(executor);
            var dbSet = StubDbSetCreator.CreateBulkOperations(operationId.ToString(), null, DateTime.Now.Year + 1);

            mockCtx.Stub(c => c.BulkOperations).Return(dbSet);

            var mockSchoolYearProvider = this.Stub<ISchoolYearContextProvider>();
            mockSchoolYearProvider.Stub(x => x.GetSchoolYear()).Return(DateTime.Now.Year);

            var sut = new BulkOperationsController(mockCreateBulkOperationAndGetById, null, mockSchoolYearProvider);

            sut.Get(operationId.ToString()).ShouldBeType<NotFoundResult>();
        }

        [Test]
        public void Should_return_resourse_when_id_and_year_are_valid()
        {
            var expected = new BulkOperationResource();

            var createBulkOperationAndGetByIdMock = MockRepository.GenerateMock<ICreateBulkOperationAndGetById>();

            createBulkOperationAndGetByIdMock.Expect(x => x.GetByIdAndYear((Arg<string>.Is.Anything), (Arg<int>.Is.Anything))).IgnoreArguments().Return(expected);
            var mockSchoolYearProvider = this.Stub<ISchoolYearContextProvider>();
            mockSchoolYearProvider.Stub(x => x.GetSchoolYear()).Return(DateTime.Now.Year);

            var sut = new BulkOperationsController(createBulkOperationAndGetByIdMock, null, mockSchoolYearProvider);

            var actual = sut.Get("Doesn't matter") as OkNegotiatedContentResult<BulkOperationResource>;

            actual.ShouldNotBeNull();

            actual.Content.ShouldEqual(expected);

            createBulkOperationAndGetByIdMock.AssertWasCalled(x => x.GetByIdAndYear(Arg<string>.Is.Anything, (Arg<int>.Is.Anything)));
        }
    }

    [TestFixture]
    public class When_calling_post_on_bulk_operations_controller : TestBase
    {
        [Test]
        public void Should_return_bad_request_when_request_is_null()
        {
            var commandFactoryMock = MockRepository.GenerateMock<ICreateBulkOperationCommandFactory>();
            var mockSchoolYearProvider = this.Stub<ISchoolYearContextProvider>();
            mockSchoolYearProvider.Stub(x => x.GetSchoolYear()).Return(DateTime.Now.Year);

            commandFactoryMock.Expect(x => x.Create(null))
                .IgnoreArguments()
                .Return(new CreateBulkOperationCommand
                {
                    ValidationErrors = new[] { "I dont't like a null request" }
                });

            var sut = new BulkOperationsController(null, commandFactoryMock, mockSchoolYearProvider);

            sut.Post(null).ShouldBeType<BadRequestErrorMessageResult>();
        }

        [Test]
        public void Should_return_bad_request_when_request_is_invalid()
        {
            var commandFactoryMock = MockRepository.GenerateMock<ICreateBulkOperationCommandFactory>();
            commandFactoryMock.Expect(x => x.Create(Arg<BulkOperationCreateRequest>.Is.Anything))
                .IgnoreArguments()
                .Return(new CreateBulkOperationCommand
                    {
                        ValidationErrors = new []{"I didn't like the request"}
                    });
            var mockSchoolYearProvider = this.Stub<ISchoolYearContextProvider>();
            mockSchoolYearProvider.Stub(x => x.GetSchoolYear()).Return(DateTime.Now.Year);

            var sut = new BulkOperationsController(null, commandFactoryMock, mockSchoolYearProvider);
            var invalidRequest = new BulkOperationCreateRequest();

            sut.Post(invalidRequest).ShouldBeType<BadRequestErrorMessageResult>();
        }


        [Test]
        public void Should_return_resource_when_request_is_valid()
        {
            var createBulkOperationAndGetByIdMock = MockRepository.GenerateMock<ICreateBulkOperationAndGetById>();

            var createBulkOperationCommandFactoryMock = MockRepository.GenerateMock<ICreateBulkOperationCommandFactory>();

            createBulkOperationCommandFactoryMock.Expect(x => x.Create(Arg<BulkOperationCreateRequest>.Is.Anything))
                                                 .IgnoreArguments()
                                                 .Return(new CreateBulkOperationCommand
                                                     {
                                                         Resource = new BulkOperationResource()
                                                     });
            var mockSchoolYearProvider = this.Stub<ISchoolYearContextProvider>();
            mockSchoolYearProvider.Stub(x => x.GetSchoolYear()).Return(DateTime.Now.Year);

            var sut = new BulkOperationsController(createBulkOperationAndGetByIdMock, createBulkOperationCommandFactoryMock, mockSchoolYearProvider);

            var request = new BulkOperationCreateRequest
            {
                UploadFiles = new[]
                    {
                        new UploadFileRequest
                            {
                                Format = "text/xml",
                                InterchangeType = InterchangeType.EducationOrganization.Name,
                            },
                    }
            };

            var response = sut.Post(request);
            response.ShouldBeType<CreatedAtRouteNegotiatedContentResult<BulkOperationResource>>();
        }
    }
}
