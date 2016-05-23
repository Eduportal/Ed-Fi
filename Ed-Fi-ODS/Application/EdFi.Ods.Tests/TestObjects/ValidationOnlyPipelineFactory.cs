using EdFi.Ods.Api.Pipelines.Factories;

namespace EdFi.Ods.Tests.TestObjects
{
    using System;
    using global::EdFi.Ods.Pipelines.Steps;

    public class ValidationOnlyGetPipelineStepsProvider : IGetPipelineStepsProvider
    {
        public Type[] GetSteps()
        {
            return new[]
            {
                typeof(MapResourceModelToEntityModel<,,,>),
                typeof(ValidateEntityModel<,,,>),
            };
        }
    }

    public class ValidationOnlyPutPipelineStepsProvider : IPutPipelineStepsProvider
    {
        public Type[] GetSteps()
        {
            return new[]
            {
                typeof(MapResourceModelToEntityModel<,,,>),
                typeof(ValidateEntityModel<,,,>),
            };
        }
    }

    public class ValidationOnlyDeletePipelineStepsProvider : IDeletePipelineStepsProvider
    {
        public Type[] GetSteps()
        {
            return new[]
            {
                typeof(MapResourceModelToEntityModel<,,,>),
                typeof(ValidateEntityModel<,,,>),
            };
        }
    }

    public class ValidationOnlyGetBySpecificationPipelineStepsProvider : IGetBySpecificationPipelineStepsProvider
    {
        public Type[] GetSteps()
        {
            return new[]
                {
                    typeof(MapResourceModelToEntityModel<,,,>),
                    typeof(ValidateEntityModel<,,,>),
                };
        }
    }
}