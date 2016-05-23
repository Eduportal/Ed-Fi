using System;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using EdFi.Common.Messaging;
using log4net;

namespace EdFi.Messaging.MSMQ
{
    public class QueueListener<TCommand> : IQueueListener<TCommand> where TCommand : ICommand
    {
        private readonly IQueueLocator _queueLocator;
        private readonly IManagerFactory<TCommand> _managerFactory;
        private readonly ILog _logger = LogManager.GetLogger(typeof(QueueListener<TCommand>));
        private CancellationTokenSource _cancelReceiveToken;

        public QueueListener(IQueueLocator queueLocator, IManagerFactory<TCommand> managerFactory)
        {
            _queueLocator = queueLocator;
            _managerFactory = managerFactory;
        }

        public void StartListening(Action<IMessageManager<TCommand>> callback)
        {
            _cancelReceiveToken = new CancellationTokenSource();
            Task.Run(() => Listen(callback, _cancelReceiveToken.Token));
        }

        public void StopListening()
        {
            _cancelReceiveToken.Cancel();
        }

        private void Listen(Action<IMessageManager<TCommand>> callback, CancellationToken token)
        {
            using (var queue = _queueLocator.GetQueue<TCommand>())
            {
                while (!token.IsCancellationRequested)
                {
                    using (var transaction = new MessageQueueTransaction())
                    {
                        transaction.Begin();
                        try
                        {
                            var message = queue.Receive(TimeSpan.FromSeconds(1), transaction);

                            if (message != null)
                            {
                                var manager = _managerFactory.Create(message, transaction);
                                callback(manager);
                            }

                            if (transaction.Status != MessageQueueTransactionStatus.Committed) transaction.Commit();
                        }
                        catch (Exception exception)
                        {
                            _logger.Error(string.Format("An exception was thrown while trying to transactionally receive a message on {0} queue: {1}", queue.FormatName, exception.Message));
                            if (transaction.Status != MessageQueueTransactionStatus.Committed) transaction.Abort();
                        }
                    }
                }
            }
        }
    }
}