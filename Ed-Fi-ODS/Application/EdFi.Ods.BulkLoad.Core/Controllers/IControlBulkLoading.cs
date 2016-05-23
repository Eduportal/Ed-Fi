using System.Threading.Tasks;
using EdFi.Ods.Messaging.BulkLoadCommands;

namespace EdFi.Ods.BulkLoad.Core.Controllers
{
    public interface IControlBulkLoading
    {
        Task Handle(StartOperationCommand command);
    }
}