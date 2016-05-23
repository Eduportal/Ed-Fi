using EdFi.Ods.Pipelines.Common;

namespace EdFi.Ods.Pipelines
{
    public abstract class PipelineBase<TContext, TResult>
        where TResult : PipelineResultBase, new()
    {
        private readonly IStep<TContext, TResult>[] steps;

        protected PipelineBase(IStep<TContext, TResult>[] steps)
        {
            this.steps = steps;
        }

        public TResult Process(TContext context)
        {
            var result = new TResult();

            foreach (var step in steps)
            {
                step.Execute(context, result);

                // If we have experienced an exception, quit processing steps now
                if (result.Exception != null)
                    break;
            }

            return result;
        }
    }
}