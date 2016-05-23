using System;
using System.Messaging;
using System.Text.RegularExpressions;
using EdFi.Common.Configuration;
using EdFi.Common.Messaging;

namespace EdFi.Messaging.MSMQ
{
    public class QueueLocator : IQueueLocator
    {
        private readonly IQueueNameProvider _nameProvider;
        private readonly IConfigValueProvider _config;

        public QueueLocator(IQueueNameProvider nameProvider, IConfigValueProvider config)
        {
            _nameProvider = nameProvider;
            _config = config;
        }

        public MessageQueue GetQueue<TCommand>(IEnvelope<TCommand> envelope) where TCommand : ICommand
        {
            return GetQueue<TCommand>();
        }

        public MessageQueue GetQueue<TCommand>() where TCommand : ICommand
        {
            var name = _nameProvider.GetQueueNameFor<TCommand>();
            var endpoint = _config.GetValue(string.Format("{0}MessageEndpoint", typeof(TCommand).Name));

            return IsLocal(endpoint) ? GetLocalQueue(name) : GetRemoteQueue(endpoint, name);
        }

        private bool IsLocal(string endpoint)
        {
            var localRegex = new Regex(@"^\.$|^localhost$", RegexOptions.IgnoreCase);
            return localRegex.IsMatch(endpoint);
        }

        private MessageQueue GetLocalQueue(string name)
        {
            var path = string.Format(@".\PRIVATE$\{0}", name);
            if(MessageQueue.Exists(path)) return GetQueueBasedOnPath(path);

            if (!_config.GetValue("QueueAutoCreate").Equals("1"))
                throw new SystemException(
                    string.Format(
                        @"The local queue for {0} messages does not appear to exist and QueueAutoCreate was not set to 1 (true).
                                                    Please create the queue or set QueueAutoCreate to 1.", name));
            MessageQueue.Create(path, true);
            var queue = GetQueueBasedOnPath(path);
            queue.SetPermissions("Everyone", MessageQueueAccessRights.ReceiveMessage, AccessControlEntryType.Allow);
            queue.SetPermissions("Everyone", MessageQueueAccessRights.WriteMessage, AccessControlEntryType.Allow);

            return queue;
        }

        private MessageQueue GetRemoteQueue(string endpoint, string name)
        {
            //This assumes the use of either UNC or DNS names for endpoints and a Windows Operating System.
            var path = string.Format(@"DIRECT=OS:{0}\PRIVATE$\{1}", endpoint, name);
            var formatName = string.Format("FormatName:{0}", path);
            try
            {
                if (MessageQueue.Exists(formatName)) return GetQueueBasedOnPath(formatName);
            }
            catch (InvalidOperationException)
            {
                throw new SystemException(string.Format(
                    "The endpoint {0} does not appear to exist or is not responding.  Please confirm the endpoint exists and has MSMQ installed with the expected queue ({1}",
                    endpoint, name));
            }
            if(MessageQueue.Exists(formatName)) return GetQueueBasedOnPath(formatName);  //The MessageQueue constructor only accepts path names in the FormatName syntax

            throw new SystemException(string.Format(
                @"The remote queue for {0} messages on endpoint {1} does not appear to exist.  
                Please confirm this queue exists (check the endpoint name for errors), create it, or run the handler on that endpoint with QueueAutoCreate set to 1 (true).
                Remote private queues must be created locally.  If this queue is supposed to be local to this system please set the endpoint to '.' or 'localhost'."
                , name, endpoint));
        }

        private MessageQueue GetQueueBasedOnPath(string path)
        {
            return new MessageQueue(path){Formatter = new JSonMessageFormatter()};
        }
    }
}