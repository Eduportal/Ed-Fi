using EdFi.Common.Messaging;
namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{

    public class StubBus : ICommandSender
    {
        private object _lastSentCommand;

        public void Send<T>(T command) where T : ICommand
        {
            _lastSentCommand = command;
        }

        public object GetLastSentCommand()
        {
            return _lastSentCommand;
        }
    }
}