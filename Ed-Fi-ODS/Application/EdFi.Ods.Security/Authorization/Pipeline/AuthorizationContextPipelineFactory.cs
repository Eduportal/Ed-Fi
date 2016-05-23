using System;
using System.Linq;
using EdFi.Common.Extensions;
using EdFi.Ods.Api.Pipelines.Factories;

namespace EdFi.Ods.Security.Authorization.Pipeline
{
    public class AuthorizationContextGetPipelineStepsProviderDecorator : IGetPipelineStepsProvider
    {
        private readonly IGetPipelineStepsProvider _next;

        public AuthorizationContextGetPipelineStepsProviderDecorator(IGetPipelineStepsProvider next)
        {
            _next = next;
        }

        public Type[] GetSteps()
        {
            return _next.GetSteps()
                .InsertAtHead(typeof(SetAuthorizationContextForGet<,,,>))
                .ToArray();
        }
    }
    
    public class AuthorizationContextGetByKeyPipelineStepsProviderDecorator : IGetByKeyPipelineStepsProvider
    {
        private readonly IGetByKeyPipelineStepsProvider _next;

        public AuthorizationContextGetByKeyPipelineStepsProviderDecorator(IGetByKeyPipelineStepsProvider next)
        {
            _next = next;
        }

        public Type[] GetSteps()
        {
            return _next.GetSteps()
                .InsertAtHead(typeof(SetAuthorizationContextForGet<,,,>))
                .ToArray();
        }
    }

    public class AuthorizationContextGetBySpecificationPipelineStepsProviderDecorator : IGetBySpecificationPipelineStepsProvider
    {
        private readonly IGetBySpecificationPipelineStepsProvider _next;

        public AuthorizationContextGetBySpecificationPipelineStepsProviderDecorator(IGetBySpecificationPipelineStepsProvider next)
        {
            _next = next;
        }

        public Type[] GetSteps()
        {
            return _next.GetSteps()
                .InsertAtHead(typeof(SetAuthorizationContextForGet<,,,>))
                .ToArray();
        }
    }

    public class AuthorizationContextPutPipelineStepsProviderDecorator : IPutPipelineStepsProvider
    {
        private readonly IPutPipelineStepsProvider _next;

        public AuthorizationContextPutPipelineStepsProviderDecorator(IPutPipelineStepsProvider next)
        {
            _next = next;
        }

        public Type[] GetSteps()
        {
            return _next.GetSteps()
                .InsertAtHead(typeof(SetAuthorizationContextForPut<,,,>))
                .ToArray();
        }
    }

    public class AuthorizationContextDeletePipelineStepsProviderDecorator : IDeletePipelineStepsProvider
    {
        private readonly IDeletePipelineStepsProvider _next;

        public AuthorizationContextDeletePipelineStepsProviderDecorator(IDeletePipelineStepsProvider next)
        {
            _next = next;
        }

        public Type[] GetSteps()
        {
            return _next.GetSteps()
                .InsertAtHead(typeof(SetAuthorizationContextForDelete<,,,>))
                .ToArray();
        }
    }
}