namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System.Collections.Generic;

    public interface IInterchangeMetadata
    {
        string InterchangeName { get; }
        IEnumerable<IManageEntityMetadata> Aggregates { get; }
    }
}