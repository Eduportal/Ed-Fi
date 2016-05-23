namespace EdFi.Ods.Tests.EdFi.Ods.Repositories.Common
{
    // TODO: GKM - review these tests
    //[TestFixture]
    //public class When_creating_a_bulk_operation_command_and_request_is_invalid
    //{
    //    [Test]
    //    public void Should_return_an_invalid_command()
    //    {
    //        var request = new BulkOperationCreateRequest();

    //        var validatorMock = MockRepository.GenerateMock<IValidator<BulkOperationCreateRequest>>();
    //        validatorMock.Stub(x => x.Validate(request))
    //                     .Return(new ValidationResult(new List<ValidationFailure> { new ValidationFailure("SomeProperty", "Fake error") }));

    //        var databaseCatalog = MockRepository.GenerateMock<IDatabaseCatalog>();

    //        var factory = new CreateBulkOperationCommandFactory(validatorMock, databaseCatalog);
    //        var command = factory.Create(request);

    //        command.Invalid.ShouldBeTrue();
    //    }
    //}
}