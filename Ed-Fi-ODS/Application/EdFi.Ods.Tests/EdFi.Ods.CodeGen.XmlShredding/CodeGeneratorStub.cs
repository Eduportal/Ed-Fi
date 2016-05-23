namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.XmlShredding
{
    using System;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.CodeGen.XmlShredding.CodeDeclarations;

    public class CodeGeneratorStub : CodeGenerator
    {
        public NamespaceDeclaration generatedNamespaceDeclaration { get; private set; }
        public CodeGeneratorStub(FakeT4 t4, FileManagerStub fileManager, CodeGenerationConfig config)
            : base(t4, fileManager, config)
        {
        }

        public override void Generate(NamespaceDeclaration namespaceDeclaration)
        {
            this.generatedNamespaceDeclaration = namespaceDeclaration;

            Console.WriteLine();
            Console.WriteLine("Generated Code:");
            Console.WriteLine();

            base.Generate(namespaceDeclaration);
        }
    }
}