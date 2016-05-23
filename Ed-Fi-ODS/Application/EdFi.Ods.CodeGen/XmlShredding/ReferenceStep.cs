namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System;
    using System.Collections.Generic;

    public class ReferenceStep : IReferenceStep
    {
        private readonly string[] _xpath;
        private readonly IDictionary<string, IMapStep> _stepsIfRefIdPresent;
        private IMapStep _nextStep;

        public ReferenceStep(string[] xpath, IMapStep nextStepIfNoIdRef, IDictionary<string, IMapStep> stepsIfRefIdPresent)
        {
            this._xpath = xpath;
            this._nextStep = nextStepIfNoIdRef;
            this._stepsIfRefIdPresent = stepsIfRefIdPresent;
        }

        public string[] GetXPath()
        {
            return this._xpath;
        }

        public IMapStep GetNextStep()
        {
            return this._nextStep;
        }

        public bool IsTerminal()
        {
            return this._nextStep == null;
        }

        public IEnumerable<string> GetTargetTypeNames()
        {
            return this._stepsIfRefIdPresent.Keys;
        }

        public void ReferencePointedToAggregateType(string type, string prefix)
        {
            IMapStep step;
            var typeWithPrefix = prefix + "-" + type;
            if (this._stepsIfRefIdPresent.TryGetValue(type, out step))
                this._nextStep = this._stepsIfRefIdPresent[type];
            else if (!string.IsNullOrWhiteSpace(prefix) && this._stepsIfRefIdPresent.TryGetValue(typeWithPrefix, out step))
                this._nextStep = this._stepsIfRefIdPresent[typeWithPrefix];
            else
                throw new Exception(string.Format("Unable to resolve next step by type '{0}'.", type));
        }
    }
}