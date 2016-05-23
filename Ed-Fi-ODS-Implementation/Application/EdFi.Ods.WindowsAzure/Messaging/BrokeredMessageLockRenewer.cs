using System;
using System.Configuration;
using System.Threading;
using Microsoft.ServiceBus.Messaging;

namespace EdFi.Ods.WindowsAzure.Messaging
{
    public class BrokeredMessageLockRenewer : IDisposable
    {
        private BrokeredMessage _message;

        private readonly object _lock = new object();
        private readonly Timer _timer;
        private bool _stopped;

        private BrokeredMessageLockRenewer(BrokeredMessage message)
        {
            if (message == null)
                throw new ArgumentException("A message must be supplied.", "message");
            _message = message;

            var refreshSeconds = ConfigurationManager.AppSettings["AzureMessageLockRefreshIntervalSeconds"];
            if (string.IsNullOrWhiteSpace(refreshSeconds))
                throw new Exception("AzureMessageLockRefreshIntervalSeconds is a required setting.");

            var interval = TimeSpan.FromSeconds(int.Parse(refreshSeconds));
            TimerCallback timerCallback = (stateInfo) => RenewLock();
            _timer = new Timer(timerCallback, this, interval, interval);
        }

        public static BrokeredMessageLockRenewer KeepMessageAlive(BrokeredMessage message)
        {
            var babySitter = new BrokeredMessageLockRenewer(message);
            return babySitter;
        }

        public void RenewLock()
        {
            lock (_lock)
            {
                if (!_stopped)
                    _message.RenewLock();
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (!_stopped)
                {
                    _timer.Dispose();
                    _message = null;
                    _stopped = true;
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}