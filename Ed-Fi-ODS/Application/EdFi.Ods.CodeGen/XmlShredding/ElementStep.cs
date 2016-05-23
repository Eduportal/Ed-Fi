namespace EdFi.Ods.CodeGen.XmlShredding
{
    public class ElementStep : IMapStep
    {
        private readonly string[] _xpath;
        private readonly IMapStep _nextStep;

        public ElementStep(string[] xpath, IMapStep nextStep)
        {
            this._xpath = xpath;
            this._nextStep = nextStep;
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
    }
}