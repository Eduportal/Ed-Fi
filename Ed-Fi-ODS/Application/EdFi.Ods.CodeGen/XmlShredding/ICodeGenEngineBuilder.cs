namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System.Collections.Generic;

    public interface ICodeGenEngineBuilder
    {
        CodeGenEngine BuildCodeGenEngine(dynamic t4, dynamic fileManager, IEnumerable<IInterchangeMetadata> interchangeMetaManagers);
    }
}