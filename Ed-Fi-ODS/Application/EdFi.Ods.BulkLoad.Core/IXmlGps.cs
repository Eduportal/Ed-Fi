using System.Collections.Generic;

namespace EdFi.Ods.BulkLoad.Core
{
    public interface IXmlGPS
    {
        string GetRawPathToCurrent();
        string GetXPathToCurrent();
        IEnumerable<string> GetReferencedAggregateTypes();
        void GoToNextStep();
        bool CurrentStepIsAReference();
        bool CurrentStepIsTerminal();
        void ReferenceWasToAggregate(string name);
    }
}