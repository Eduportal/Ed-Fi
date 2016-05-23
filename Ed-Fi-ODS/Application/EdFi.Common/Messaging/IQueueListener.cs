using System;

namespace EdFi.Common.Messaging
{
    public interface IQueueListener<TCommand> where TCommand : ICommand
    {
        void StartListening(Action<IMessageManager<TCommand>> callback);
        void StopListening();
    }
}