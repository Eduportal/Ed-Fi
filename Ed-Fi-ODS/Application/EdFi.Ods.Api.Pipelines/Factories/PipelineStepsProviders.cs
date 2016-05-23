// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************
using System;
using EdFi.Ods.Pipelines.Steps;

namespace EdFi.Ods.Api.Pipelines.Factories
{
    /// <summary>
    /// Provides the core Ed-Fi ODS API steps for "GetById" access.
    /// </summary>
    public class GetPipelineStepsProvider : IGetPipelineStepsProvider
    {
        /// <summary>
        /// Provides the core Ed-Fi ODS API steps for "GetById" access.
        /// </summary>
        public Type[] GetSteps()
        {
            return new[]
            {
                typeof(GetEntityModelById<,,,>),
                typeof(DetectUnmodifiedEntityModel<,,,>),
                typeof(MapEntityModelToResourceModel<,,,>),
            };
        }
    }

    /// <summary>
    /// Provides the core Ed-Fi ODS API steps for "GetBySpecification" access.
    /// </summary>
    public class GetBySpecificationPipelineStepsProvider : IGetBySpecificationPipelineStepsProvider
    {
        public Type[] GetSteps()
        {
            return new[]
            {
                typeof(MapResourceModelToEntityModel<,,,>),
                typeof(GetEntityModelsBySpecification<,,,>),
                typeof(MapEntityModelsToResourceModels<,,,>),
            };
        }
    }

    /// <summary>
    /// Provides the core Ed-Fi ODS API steps for "GetByKey" access.
    /// </summary>
    public class GetByKeyPipelineStepsProvider : IGetByKeyPipelineStepsProvider
    {
        public Type[] GetSteps()
        {
            return new[]
            {
                typeof(MapResourceModelToEntityModel<,,,>),
                typeof(GetEntityModelByKey<,,,>),
                typeof(MapEntityModelToResourceModel<,,,>),
            };
        }
    }

    /// <summary>
    /// Provides the core Ed-Fi ODS API steps for "Upsert" persistence.
    /// </summary>
    public class PutPipelineStepsProvider : IPutPipelineStepsProvider
    {
        public virtual Type[] GetSteps()
        {
            return new[]
            {
                typeof(ValidateResourceModel<,,,>),
                typeof(MapResourceModelToEntityModel<,,,>),
                typeof(PersistEntityModel<,,,>),
            };
        }
    }

    /// <summary>
    /// Provides the core Ed-Fi ODS API steps for "Delete" persistence.
    /// </summary>
    public class DeletePipelineStepsProvider : IDeletePipelineStepsProvider
    {
        public Type[] GetSteps()
        {
            return new[]
            {
                typeof(DeleteEntityModel<,,,>),
            };
        }
    }
}