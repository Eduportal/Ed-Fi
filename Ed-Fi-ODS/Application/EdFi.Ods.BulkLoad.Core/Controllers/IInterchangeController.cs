using System.Threading.Tasks;

namespace EdFi.Ods.BulkLoad.Core.Controllers
{
    public interface IInterchangeController
    {
        Task<LoadResult> LoadAsync(string fromFile);
    }
}