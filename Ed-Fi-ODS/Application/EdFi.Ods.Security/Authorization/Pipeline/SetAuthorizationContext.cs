using EdFi.Common.Security.Claims;
using EdFi.Ods.Pipelines.Common;
using EdFi.Ods.Security.Metadata.Repositories;

namespace EdFi.Ods.Security.Authorization.Pipeline
{
    /// <summary>
    /// Sets the resource and action values into the current context for downstream authorization activities.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TResourceModel"></typeparam>
    /// <typeparam name="TEntityModel"></typeparam>
    public abstract class SetAuthorizationContextBase<TContext, TResult, TResourceModel, TEntityModel> 
        : IStep<TContext, TResult>
    {
        protected readonly IAuthorizationContextProvider AuthorizationContextProvider;
        protected readonly ISecurityRepository SecurityRepository;
        protected abstract string Action { get; }

        protected SetAuthorizationContextBase(IAuthorizationContextProvider authorizationContextProvider, ISecurityRepository securityRepository)
        {
            AuthorizationContextProvider = authorizationContextProvider;
            SecurityRepository = securityRepository;
        }

        public virtual void Execute(TContext context, TResult result)
        {
            AuthorizationContextProvider.SetResource(typeof(TResourceModel).GetResourceName());
            AuthorizationContextProvider.SetAction(Action);
        }
    }

    /// <summary>
    /// Sets the action to Read in context for downstream authorization processing.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TResourceModel"></typeparam>
    /// <typeparam name="TEntityModel"></typeparam>
    public class SetAuthorizationContextForGet<TContext, TResult, TResourceModel, TEntityModel> 
        : SetAuthorizationContextBase<TContext, TResult, TResourceModel, TEntityModel>
    {
        public SetAuthorizationContextForGet(IAuthorizationContextProvider authorizationContextProvider, ISecurityRepository securityRepository) 
            : base(authorizationContextProvider, securityRepository) { }

        protected override string Action
        {
            get { return SecurityRepository.GetActionByName("Read").ActionUri; }
        }
    }

    /// <summary>
    /// Sets the action to Update in context for downstream authorization processing.
    /// </summary>
    public class SetAuthorizationContextForPut<TContext, TResult, TResourceModel, TEntityModel> 
        : SetAuthorizationContextBase<TContext, TResult, TResourceModel, TEntityModel>
    {
        public SetAuthorizationContextForPut(IAuthorizationContextProvider authorizationContextProvider, ISecurityRepository securityRepository) 
            : base(authorizationContextProvider, securityRepository) { }

        protected override string Action
        {
            get { return SecurityRepository.GetActionByName("Update").ActionUri; }
        }
    }

    /// <summary>
    /// Sets the action to Upsert in context for downstream authorization processing.
    /// </summary>
    public class SetAuthorizationContextForPost<TContext, TResult, TResourceModel, TEntityModel>
        : SetAuthorizationContextBase<TContext, TResult, TResourceModel, TEntityModel>
    {
        public SetAuthorizationContextForPost(IAuthorizationContextProvider authorizationContextProvider, ISecurityRepository securityRepository)
            : base(authorizationContextProvider, securityRepository) { }

        protected override string Action
        {
            get { return "Upsert"; }
        }
    }

    /// <summary>
    /// Sets the action to Delete in context for downstream authorization processing.
    /// </summary>
    public class SetAuthorizationContextForDelete<TContext, TResult, TResourceModel, TEntityModel> 
        : SetAuthorizationContextBase<TContext, TResult, TResourceModel, TEntityModel>
    {
        public SetAuthorizationContextForDelete(IAuthorizationContextProvider authorizationContextProvider, ISecurityRepository securityRepository)
            : base(authorizationContextProvider, securityRepository) { }

        protected override string Action
        {
            get { return SecurityRepository.GetActionByName("Delete").ActionUri; }
        }
    }
}
