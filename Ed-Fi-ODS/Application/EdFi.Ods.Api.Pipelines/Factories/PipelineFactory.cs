using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.Api.Pipelines.Factories;
using EdFi.Ods.Common;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;
using EdFi.Ods.Pipelines.Delete;
using EdFi.Ods.Pipelines.Get;
using EdFi.Ods.Pipelines.GetByKey;
using EdFi.Ods.Pipelines.GetMany;
using EdFi.Ods.Pipelines.Put;

namespace EdFi.Ods.Pipelines.Factories
{
    public class PipelineFactory : IPipelineFactory
    {
        private readonly IServiceLocator _locator;
        private readonly IGetPipelineStepsProvider getPipelineStepsProvider;
        private readonly IGetByKeyPipelineStepsProvider getByKeyPipelineStepsProvider;
        private readonly IGetBySpecificationPipelineStepsProvider getBySpecificationPipelineStepsProvider;
        private readonly IPutPipelineStepsProvider putPipelineStepsProvider;
        private readonly IDeletePipelineStepsProvider deletePipelineStepsProvider;

        public PipelineFactory( IServiceLocator locator,
            IGetPipelineStepsProvider getPipelineStepsProvider, 
            IGetByKeyPipelineStepsProvider getByKeyPipelineStepsProvider,
            IGetBySpecificationPipelineStepsProvider getBySpecificationPipelineStepsProvider,
            IPutPipelineStepsProvider putPipelineStepsProvider,
            IDeletePipelineStepsProvider deletePipelineStepsProvider)
        {
            this._locator = locator;
            this.getPipelineStepsProvider = getPipelineStepsProvider;
            this.getByKeyPipelineStepsProvider = getByKeyPipelineStepsProvider;
            this.getBySpecificationPipelineStepsProvider = getBySpecificationPipelineStepsProvider;
            this.putPipelineStepsProvider = putPipelineStepsProvider;
            this.deletePipelineStepsProvider = deletePipelineStepsProvider;
        }

        public GetPipeline<TResourceModel, TEntityModel> CreateGetPipeline<TResourceModel, TEntityModel>()
            where TResourceModel : IHasETag
            where TEntityModel : class
        {
            var stepTypes = getPipelineStepsProvider.GetSteps();
            var steps = ResolvePipelineSteps<GetContext<TEntityModel>, GetResult<TResourceModel>, TResourceModel, TEntityModel>(stepTypes);
            return new GetPipeline<TResourceModel, TEntityModel>(steps);
        }

        public GetByKeyPipeline<TResourceModel, TEntityModel> CreateGetByKeyPipeline<TResourceModel, TEntityModel>()
            where TResourceModel : IHasETag
            where TEntityModel : class
        {
            var stepTypes = getByKeyPipelineStepsProvider.GetSteps();
            var steps = ResolvePipelineSteps<GetByKeyContext<TResourceModel, TEntityModel>, GetByKeyResult<TResourceModel>, TResourceModel, TEntityModel>(stepTypes);
            return new GetByKeyPipeline<TResourceModel, TEntityModel>(steps);
        }
        public GetManyPipeline<TResourceModel, TEntityModel> CreateGetManyPipeline<TResourceModel, TEntityModel>()
            where TResourceModel : IHasETag
            where TEntityModel : class
        {
            var stepTypes = getBySpecificationPipelineStepsProvider.GetSteps();
            var steps = ResolvePipelineSteps<GetManyContext<TResourceModel, TEntityModel>, GetManyResult<TResourceModel>, TResourceModel, TEntityModel>(stepTypes);
            return new GetManyPipeline<TResourceModel, TEntityModel>(steps);
        }

        public PutPipeline<TResourceModel, TEntityModel> CreatePutPipeline<TResourceModel, TEntityModel>()
            where TEntityModel : class, IHasIdentifier, new()
            where TResourceModel : IHasETag
        {
            var stepTypes = putPipelineStepsProvider.GetSteps();
            var steps = ResolvePipelineSteps<PutContext<TResourceModel, TEntityModel>, PutResult, TResourceModel, TEntityModel>(stepTypes);
            return new PutPipeline<TResourceModel, TEntityModel>(steps);
        }

        public DeletePipeline CreateDeletePipeline<TResourceModel, TEntityModel>()
        {
            var stepTypes = deletePipelineStepsProvider.GetSteps();
            var steps = ResolvePipelineSteps<DeleteContext, DeleteResult, TResourceModel, TEntityModel>(stepTypes);
            return new DeletePipeline(steps);
        }

        private IStep<TContext, TResult>[] ResolvePipelineSteps<TContext, TResult, TResourceModel, TEntityModel>(IEnumerable<Type> stepTypes)
        {
            var pipelineStepTypes = new List<IStep<TContext, TResult>>();

            foreach (var pipelineStepType in stepTypes)
            {
                Type resolutionType;

                if (pipelineStepType.IsGenericTypeDefinition)
                {
                    var typeArgsNames = pipelineStepType.GetGenericArguments().Select(x => x.Name);

                    var typeArgs = GetGenericTypes<TContext, TResult, TResourceModel, TEntityModel>(typeArgsNames);

                    resolutionType = pipelineStepType.MakeGenericType(typeArgs.ToArray());
                }
                else
                {
                    resolutionType = pipelineStepType;
                }

                pipelineStepTypes.Add((IStep<TContext, TResult>) _locator.Resolve(resolutionType));
            }

            return pipelineStepTypes.ToArray();
        }

        private IEnumerable<Type> GetGenericTypes<TContext, TResult, TResourceModel, TEntityModel>(IEnumerable<string> genericArgNames)
        {
            foreach (var genericArgName in genericArgNames)
            {
                switch (genericArgName)
                {
                    case "TContext":
                        yield return typeof (TContext);
                        break;

                    case "TResult":
                        yield return typeof (TResult);
                        break;

                    case "TResourceModel":
                        yield return typeof (TResourceModel);
                        break;

                    case "TEntityModel":
                        yield return typeof (TEntityModel);
                        break;

                    default:
                        // Defensive programming
                        throw new Exception(string.Format("Unsupported generic type argument name '{0}'.", genericArgName));
                }
            }
        }
    }
}