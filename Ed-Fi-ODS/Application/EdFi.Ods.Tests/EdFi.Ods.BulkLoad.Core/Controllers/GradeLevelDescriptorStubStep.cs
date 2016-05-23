namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using global::EdFi.Ods.Api.Models.Resources.GradeLevelDescriptor;
    using global::EdFi.Ods.Pipelines.Common;
    using global::EdFi.Ods.Pipelines.Put;

    public class GradeLevelDescriptorStubStep : IStep<PutContext<GradeLevelDescriptor, global::EdFi.Ods.Entities.NHibernate.GradeLevelDescriptorAggregate.GradeLevelDescriptor>, PutResult>
    {
        private GradeLevelDescriptor _lastDescriptorExecuted;

        public GradeLevelDescriptor GiveMeLastExecutedDescriptor()
        {
            return this._lastDescriptorExecuted;
        }

        public void Execute(PutContext<GradeLevelDescriptor, global::EdFi.Ods.Entities.NHibernate.GradeLevelDescriptorAggregate.GradeLevelDescriptor> context, PutResult result)
        {
            this._lastDescriptorExecuted = context.Resource;
        }
    }
}