using System;
using System.Globalization;
using EdFi.Common.Messaging;
using Microsoft.ServiceBus.Messaging;

namespace EdFi.Ods.WindowsAzure.Messaging
{
    public class AzureMessageManager<TCommand> : IDisposable, IMessageManager<TCommand> where TCommand : ICommand 
    {
        private readonly BrokeredMessage brokeredMessage;
        private readonly BrokeredMessageLockRenewer lockRenewer;
        private bool disposed;

        public AzureMessageManager(BrokeredMessage brokeredMessage, BrokeredMessageLockRenewer lockRenewer)
        {
            this.brokeredMessage = brokeredMessage;
            this.lockRenewer = lockRenewer;
        }

        public void Complete()
        {
            lockRenewer.Stop();
            brokeredMessage.Complete();
        }

        public string Id { get { return brokeredMessage.SequenceNumber.ToString(CultureInfo.InvariantCulture); } }

        public void DeadLetter(string reason, string description)
        {
            brokeredMessage.DeadLetter(reason, description);
        }

        public IEnvelope<TCommand> Envelope
        {
            get { return brokeredMessage.GetBody<Envelope<TCommand>>(); }
            
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing && brokeredMessage != null)
            {
                lockRenewer.Dispose();
                brokeredMessage.Dispose();
            }
            disposed = true;
        }
    }
}