namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using global::EdFi.Ods.Api.Models.Resources.GradeLevelDescriptor;
    using global::EdFi.Ods.Pipelines.Common;
    using global::EdFi.Ods.Pipelines.Put;

    public class StubPipeline : PutPipeline<GradeLevelDescriptor, global::EdFi.Ods.Entities.NHibernate.GradeLevelDescriptorAggregate.GradeLevelDescriptor>
    {
        public StubPipeline(IStep<PutContext<GradeLevelDescriptor, global::EdFi.Ods.Entities.NHibernate.GradeLevelDescriptorAggregate.GradeLevelDescriptor>, PutResult>[] steps) : base(steps)
        {
        }
    }
}