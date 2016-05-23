namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System.Collections.Generic;

    public class CodeGenEngineBuilder : ICodeGenEngineBuilder
    {
        public CodeGenEngine BuildCodeGenEngine(dynamic t4, dynamic fileManager, IEnumerable<IInterchangeMetadata> interchangeMetaManagers)
        {
            return new CodeGenEngine(new CodeGenerator(t4, fileManager, new CodeGenerationConfig()), interchangeMetaManagers);
        }
    }
}