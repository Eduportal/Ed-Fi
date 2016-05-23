namespace EdFi.Ods.CodeGen.XmlShredding.CodeDeclarations
{
    using System.Collections.Generic;

    public class NamespaceDeclaration
    {
        public string NamespaceEndingPortion { get; set; }
        public IList<string> Usings { get; set; }
        public IList<ClassDeclaration> Classes { get; set; }
        public List<InterfaceDeclaration> Interfaces { get; set; }

        public NamespaceDeclaration(string namespaceEndingPortion)
        {
            this.NamespaceEndingPortion = namespaceEndingPortion;
            this.Classes = new List<ClassDeclaration>();
            this.Interfaces = new List<InterfaceDeclaration>();
            this.Usings = new List<string>();
        }
    }
}