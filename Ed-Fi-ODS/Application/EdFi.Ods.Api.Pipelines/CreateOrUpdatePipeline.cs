// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using EdFi.Ods.Common;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines;
using EdFi.Ods.Pipelines.Factories;
using EdFi.Ods.Pipelines.Put;

namespace EdFi.Ods.Api.Pipelines
{
    public abstract class CreateOrUpdatePipeline<TResourceModel, TEntityModel> : ICreateOrUpdatePipeline<TResourceModel, TEntityModel>
        where TResourceModel : IHasETag
        where TEntityModel : class, IHasIdentifier, new()
    {
        private readonly PutPipeline<TResourceModel, TEntityModel> _pipeline;

        protected CreateOrUpdatePipeline(IPipelineFactory factory)
        {
            _pipeline = factory.CreatePutPipeline<TResourceModel, TEntityModel>();
        }

        public PutResult Process(PutContext<TResourceModel, TEntityModel> context)
        {
            return _pipeline.Process(context);
        }
    }
}