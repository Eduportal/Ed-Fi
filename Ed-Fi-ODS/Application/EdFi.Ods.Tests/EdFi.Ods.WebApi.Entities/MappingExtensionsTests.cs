namespace EdFi.Ods.Tests.EdFi.Ods.WebApi.Entities
{
    using global::EdFi.Ods.Api.Data.Mappings;
    using global::EdFi.Ods.Api.Models.Resources;
    using global::EdFi.Ods.Api.Models.Resources.Enums;
    using global::EdFi.Ods.Common.Utils.Extensions;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class When_mapping_a_bulk_operation
    {
        [Test]
        public void Should_map_all_values()
        {
            var resource = new BulkOperationResource
            {
                Id = "123",
                ResetDistrictData = "abc",
                Status = BulkOperationStatus.Started.GetName(),
                UploadFiles =
                    new[]
                    {
                        new UploadFileResource
                        {
                            Format = "xyz",
                            Id = "456",
                            InterchangeType = "xml",
                            Size = 99,
                            Status = UploadFileStatus.Expired.GetName(),
                        }
                    }
            };

            var mapped = resource.ToEntity().ToResource();

            mapped.Id.ShouldEqual(resource.Id);
            mapped.ResetDistrictData.ShouldEqual(resource.ResetDistrictData);
            mapped.Status.ShouldEqual(resource.Status);
            mapped.UploadFiles[0].Format.ShouldEqual(resource.UploadFiles[0].Format);
            mapped.UploadFiles[0].Id.ShouldEqual(resource.UploadFiles[0].Id);
            mapped.UploadFiles[0].InterchangeType.ShouldEqual(resource.UploadFiles[0].InterchangeType);
            mapped.UploadFiles[0].Size.ShouldEqual(resource.UploadFiles[0].Size);
            mapped.UploadFiles[0].Status.ShouldEqual(resource.UploadFiles[0].Status);
        }
    }
}
