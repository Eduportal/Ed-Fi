namespace EdFi.Ods.CodeGen.XmlShredding
{
    using EdFi.Ods.CodeGen.XmlShredding.CodeDeclarations;

    public class CodeGenContext
    {
        public NamespaceDeclaration NamespaceDeclaration { get; private set; }
        public ClassDeclaration ClassDeclaration { get; private set; }

        public CodeGenContext(NamespaceDeclaration namespaceDeclaration, ClassDeclaration classDeclaration)
        {
            this.NamespaceDeclaration = namespaceDeclaration;
            this.ClassDeclaration = classDeclaration;
        }
    }

}