using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Ods.Utilities.LoadGeneration.Infrastructure;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface IResourceSelector
    {
        string GetNextResourceToGenerate();
    }

    public class ResourceSelector:IResourceSelector
    {
        private IResourceCountManager _resourceCountManager;
        private readonly IRandom _random;

        public ResourceSelector(IResourceCountManager resourceCountManager, IRandom random)
        {
            _resourceCountManager = resourceCountManager;
            _random = random;
        }

        public string GetNextResourceToGenerate()
        {
            int totalRemainingToGenerate = _resourceCountManager.TotalRemainingCount;
            
            if (totalRemainingToGenerate == 0)
                return null;

            int resourceNumberToGenerate = _random.Next(1, totalRemainingToGenerate + 1);
            int runningTotal = 0;
            foreach (var resourceCount in _resourceCountManager.Resources)
            {
                runningTotal += resourceCount.Count;
                if (resourceNumberToGenerate <= runningTotal)
                    return resourceCount.ResourceName;
            }
            return null;
        }
    }
}
