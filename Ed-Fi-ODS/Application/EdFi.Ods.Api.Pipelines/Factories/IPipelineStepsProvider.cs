using System;

namespace EdFi.Ods.Api.Pipelines.Factories
{
    public interface IPipelineStepsProvider
    {
        Type[] GetSteps();
    }

    public interface IGetPipelineStepsProvider                : IPipelineStepsProvider { }
    public interface IGetByKeyPipelineStepsProvider           : IPipelineStepsProvider { }
    public interface IGetBySpecificationPipelineStepsProvider : IPipelineStepsProvider { }
    public interface IPutPipelineStepsProvider                : IPipelineStepsProvider { }
    public interface IDeletePipelineStepsProvider             : IPipelineStepsProvider { }
}