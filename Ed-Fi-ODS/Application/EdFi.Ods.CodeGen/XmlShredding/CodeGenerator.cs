namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System.Linq;

    using EdFi.Ods.CodeGen.XmlShredding.CodeDeclarations;

    public class CodeGenerationConfig
    {
        public string Indent
        {
            get { return "    "; }
        }

        public string RootNamespace
        {
            get 
            {
                return "EdFi.Ods.XmlShredding.ResourceFactories"; 
            }
        }
    }

    public interface ICodeGenerator
    {
        void Generate(NamespaceDeclaration namespaceDeclaration);
        void StartNewFile(string fileName);
        void WriteToFile();
    }

    public class CodeGenerator : ICodeGenerator
    {
        private readonly dynamic t4;
        private readonly dynamic fileManager;
        private readonly CodeGenerationConfig config;

        public CodeGenerator(dynamic t4, dynamic fileManager, CodeGenerationConfig config)
        {
            this.t4 = t4;
            this.fileManager = fileManager;
            this.config = config;
        }

        public virtual void Generate(NamespaceDeclaration namespaceDeclaration)
        {
            this.WriteLine("namespace {0}.{1}", this.config.RootNamespace, namespaceDeclaration.NamespaceEndingPortion);
            this.WriteLine("{");
            this.PushIndent();

            foreach (var @using in namespaceDeclaration.Usings)
            {
                this.WriteLine("using {0};", @using);
            }

            this.WriteLine();

            foreach (var interfaceDeclaration in namespaceDeclaration.Interfaces)
            {
                this.WriteLine("public interface {0}", interfaceDeclaration.InterfaceName + (!string.IsNullOrEmpty(interfaceDeclaration.Base) ? " : " + interfaceDeclaration.Base : string.Empty));
                this.WriteLine("{");
                this.WriteLine("}");
            }

            foreach (var classCode in namespaceDeclaration.Classes)
            {
                this.WriteLine("public class {0}", classCode.ClassName  + (!string.IsNullOrEmpty(classCode.Base) ? " : " + classCode.Base  : string.Empty) );
                this.WriteLine("{");
                this.PushIndent();

                foreach (var method in classCode.Methods)
                {
                    this.WriteLine(method.ToString());
                    this.WriteLine("{");
                    this.PushIndent();

                    foreach (var line in method.Lines)
                    {
                        this.WriteLine(this.LineContentWithIndent(line));
                    }

                    this.PopIndent();
                    this.WriteLine("}");//end of the method
                    this.WriteLine();
                }

                this.PopIndent();
                this.WriteLine("}");//end of the class
            }

            this.PopIndent();
            this.WriteLine("}");//end of the namespace
        }

        public void StartNewFile(string fileName)
        {
            this.fileManager.StartNewFile(fileName);
        }

        public void WriteToFile()
        {
            this.fileManager.Process();
        }

        private string LineContentWithIndent(LineDeclaration line)
        {
            return string.Concat(Enumerable.Repeat(this.config.Indent, line.IndentCount)) + line.Content;
        }
        private void WriteLine(string format, params object[] args)
        {
            this.t4.WriteLine(format, args);
        }

        private void WriteLine(string textToAppend)
        {
            this.t4.WriteLine(textToAppend);
        }

        private void WriteLine()
        {
            this.t4.WriteLine("");
        }


        private void PushIndent()
        {
            this.t4.PushIndent(this.config.Indent);
        }

        private void PopIndent()
        {
            this.t4.PopIndent();
        }
    }
}