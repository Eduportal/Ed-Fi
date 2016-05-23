namespace EdFi.Ods.CodeGen.XmlShredding.CodeDeclarations
{
    using System.Collections.Generic;
    using System.Linq;

    public class ClassDeclaration
    {
        private readonly Stack<MethodDeclaration> methodStack = new Stack<MethodDeclaration>();
        public string ClassName { get; set; }
        public string Base { get; set; }
        public string GenericParameter { get; set; }

        private MethodDeclaration currentMethod
        {
            get { return this.methodStack.FirstOrDefault(); }
        }

        public IList<MethodDeclaration> Methods { get; private set; }

        public ClassDeclaration()
        {
            this.Methods = new List<MethodDeclaration>();
        }


        public ClassDeclaration OpenMethod(MethodDeclaration method)
        {
            this.methodStack.Push(method);

            this.Methods.Add(method);

            return this;
        }

        public ClassDeclaration EndMethod()
        {
            this.methodStack.Pop();
            return this;
        }

        public ClassDeclaration WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(format, args));

            return this;
        }

        public ClassDeclaration WriteLine(string textToAppend)
        {

            this.currentMethod.Lines.Add(new LineDeclaration
                {
                    Content = textToAppend,
                    IndentCount = this.currentMethod.CurrentIndent
                });

            return this;
        }

        public ClassDeclaration WriteLine()
        {
            this.WriteLine("");
            return this;
        }

        public ClassDeclaration Write(string format, params object[] args)
        {
            this.Write(string.Format(format, args));

            return this;
        }

        public ClassDeclaration Write(string textToAppend)
        {
            if (!this.currentMethod.Lines.Any())
                return this.WriteLine(textToAppend);

            this.currentMethod.Lines.Last().Content += textToAppend;

            return this;
        }

        public ClassDeclaration PushIndent()
        {
            this.currentMethod.CurrentIndent++;

            return this;
        }

        public ClassDeclaration PopIndent()
        {
            this.currentMethod.CurrentIndent--;
            return this;
        }
    }
}