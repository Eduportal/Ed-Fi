using System;
using EdFi.Ods.Common;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Common;
using EdFi.Ods.Api.Common;

namespace EdFi.Ods.Pipelines.Steps
{
    using EdFi.Ods.Api.Data.EventStore;

    public class StoreEvent<TContext, TResult, TResourceModel, TEntityModel> : IStep<TContext, TResult>
        where TContext : IHasIdentifier
        where TEntityModel : class, IHasIdentifier
        where TResourceModel : class, IHasETag, new()
        where TResult : PipelineResultBase, IHasResourceChangeDetails
    {
        private readonly IEventLogRepository repository;

        public StoreEvent(IEventLogRepository repository)
        {
            this.repository = repository;
        }

        public void Execute(TContext context, TResult result)
        {
            try
            {
                

               // repository.Store(context.Id, );
                /*
                // If nothing was done to the resource as a result of this operation, then don't log anything.
                if (!result.ResourceWasCreated && !result.ResourceWasUpdated)
                    return;

                var resourceId = context.Id;

                using (var stream = repository.OpenStream(resourceId, 0, int.MaxValue))
                {
                    var contextWithResource = context as IHasResource<TResourceModel>;

                    // Use the resource as the event body unless it's not available, in which case create a 
                    // new instance of the resource type so that the event is written to the event store.
                    var eventBody = contextWithResource != null ?
                        contextWithResource.Resource
                        : new TResourceModel();

                    // Create the event message and add the headers
                    var message = new EventMessage { Body = eventBody };
                    message.Headers.Add("ApplicationKey", "TBD");
                    message.Headers.Add("EventDate", DateTime.Now);
                    message.Headers.Add("Result", "Success");
                    message.Headers.Add("Operation", typeof(TContext).Name.Replace("Context", string.Empty));

                    // Add the message to the stream
                    stream.Add(message);

                    // Persist the changes to the stream
                    stream.CommitChanges(Guid.NewGuid());
                }
                */
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
        }
    }
}
