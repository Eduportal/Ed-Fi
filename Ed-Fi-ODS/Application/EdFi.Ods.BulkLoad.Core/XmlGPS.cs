using System.Collections.Generic;
using System.Text;
using EdFi.Ods.CodeGen.XmlShredding;
using NHibernate.Linq;

namespace EdFi.Ods.BulkLoad.Core
{
    public class XmlGPS : IXmlGPS
    {
        private IMapStep _current;
        private readonly string _namespacePrefix;

        public XmlGPS(IMapStep firstStep, string namespacePrefix)
        {
            _current = firstStep;
            _namespacePrefix = namespacePrefix;
        }

        public string GetRawPathToCurrent()
        {
            var sb = new StringBuilder();
            _current.GetXPath().ForEach(n => sb.Append(n));
            return sb.ToString();
        }

        public string GetXPathToCurrent()
        {
            var sb = new StringBuilder();
            _current.GetXPath().ForEach(n => sb.Append(string.Format("/{0}:{1}", _namespacePrefix, n)));
            sb.Insert(0, "./");
            return sb.ToString();
        }

        public IEnumerable<string> GetReferencedAggregateTypes()
        {
            return _current is IReferenceStep ? ((IReferenceStep)_current).GetTargetTypeNames() : new List<string>();
        }

        public void GoToNextStep()
        {
            _current = _current.GetNextStep();
        }

        public bool CurrentStepIsAReference()
        {
            return _current is IReferenceStep;
        }

        public bool CurrentStepIsTerminal()
        {
            return _current.IsTerminal();
        }

        public void ReferenceWasToAggregate(string name)
        {
            if (_current is IReferenceStep) ((IReferenceStep)_current).ReferencePointedToAggregateType(name,_namespacePrefix);
        }
    }
}