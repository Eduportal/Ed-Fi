namespace EdFi.Ods.CodeGen.XmlShredding.CodeDeclarations
{
    using System.Collections.Generic;

    public class MethodDeclaration
    {
        private int currentIndent;
        public string AccessModifier { get; set; }
        public string ReturnType { get; set; }
        public string MethodName { get; set; }
        public string Parameters { get; set; }
        
        public MethodDeclaration(string accessModifier, string returnType, string methodName, string parameters)
        {
            this.AccessModifier = accessModifier;
            this.ReturnType = returnType;
            this.MethodName = methodName;
            this.Parameters = parameters;
            this.Lines = new List<LineDeclaration>();
        }

        public int CurrentIndent
        {
            get { return this.currentIndent; }
            set
            {
                this.currentIndent = value >= 0 ? value : 0;
            }
        }

        public IList<LineDeclaration> Lines { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}({3})", this.AccessModifier, this.ReturnType, this.MethodName, this.Parameters);
        }
       
    }
}