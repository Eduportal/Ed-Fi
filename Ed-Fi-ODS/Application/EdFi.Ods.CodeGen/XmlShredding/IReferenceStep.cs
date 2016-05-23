namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System.Collections.Generic;

    public interface IReferenceStep : IMapStep
    {
        IEnumerable<string> GetTargetTypeNames();
        void ReferencePointedToAggregateType(string type, string prefix);
    }
}