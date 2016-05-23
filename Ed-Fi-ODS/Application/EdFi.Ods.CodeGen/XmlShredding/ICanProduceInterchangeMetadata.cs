namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System.Collections.Generic;

    public interface ICanProduceInterchangeMetadata
    {
        IEnumerable<IInterchangeMetadata> Generate(IEnumerable<string> xsdFilePaths);
    }
}