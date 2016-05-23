// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Linq;
using EdFi.Common.Extensions;
using EdFi.Ods.Api.Pipelines.Factories;
using EdFi.Ods.Pipelines.Steps;

namespace EdFi.Ods.Security.Authorization.Pipeline
{
    /// <summary>
    /// Implements a <see cref="IPutPipelineStepsProvider"/> decorator that inserts a step to 
    /// populate the GUID-based Id from the supplied UniqueId for person-type resources.
    /// </summary>
    public class UniqueIdIntegrationPutPipelineStepsProviderDecorator : IPutPipelineStepsProvider
    {
        private readonly IPutPipelineStepsProvider _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueIdIntegrationPutPipelineStepsProviderDecorator"/> class using
        /// a supplied provider to be decorated.
        /// </summary>
        /// <param name="next">The decorated provider.</param>
        public UniqueIdIntegrationPutPipelineStepsProviderDecorator(IPutPipelineStepsProvider next)
        {
            _next = next;
        }

        /// <summary>
        /// Gets the types representing the steps, inserting a population step before the existing
        /// validation step.
        /// </summary>
        /// <returns></returns>
        public Type[] GetSteps()
        {
            return _next.GetSteps()
                .InsertBefore(typeof(PersistEntityModel<,,,>),
                    typeof(PopulateIdFromUniqueIdOnPeople<,,,>))
                .ToArray();
        }
    }
}