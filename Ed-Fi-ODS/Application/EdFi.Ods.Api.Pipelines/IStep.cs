namespace EdFi.Ods.Pipelines.Common
{
    public interface IStep<in TContext, in TResult>
    {   
        void Execute(TContext context, TResult result);
    }
}