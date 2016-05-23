using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Pipelines.Delete
{
    public class DeletePipeline : PipelineBase<DeleteContext, DeleteResult>
    {
        public DeletePipeline(IStep<DeleteContext, DeleteResult>[] steps)
            : base(steps)
        {
        }
    }
}