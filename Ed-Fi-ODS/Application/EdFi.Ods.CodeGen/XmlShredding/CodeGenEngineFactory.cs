namespace EdFi.Ods.CodeGen.XmlShredding
{
    using EdFi.Common.IO;

    public class CodeGenEngineFactory
    {
        public const string FileSearchPattern = "Interchange-";

        private readonly IInterchangeXsdFileProvider _xsdFileProvider;
        private readonly ICanProduceInterchangeMetadata _metadataFactory;
        private readonly ICodeGenEngineBuilder _engineBuilder;

        public CodeGenEngineFactory(IInterchangeXsdFileProvider xsdFileProvider, ICanProduceInterchangeMetadata metadataFactory, ICodeGenEngineBuilder engineBuilder)
        {
            this._xsdFileProvider = xsdFileProvider;
            this._metadataFactory = metadataFactory;
            this._engineBuilder = engineBuilder;
        }

        public static ICodeGenEngine Build(dynamic t4, dynamic fileManager, string schemaPath)
        {
            var interchangeXsdFileProvider = new InterchangeXsdFileProvider(schemaPath);
            var interchangeMetadataFactory = new InterchangeMetadataFactory();
            var codeGenEngineBuilder = new CodeGenEngineBuilder();

            var factoryInstance = new CodeGenEngineFactory(interchangeXsdFileProvider, interchangeMetadataFactory, codeGenEngineBuilder);
            return factoryInstance.BuildCore(t4, fileManager);
        }

        public ICodeGenEngine BuildCore(dynamic t4, dynamic fileManager)
        {
            var files = this._xsdFileProvider.GetInterchangeFilePaths();
            var interchangeMetaManagers = this._metadataFactory.Generate(files);
            return this._engineBuilder.BuildCodeGenEngine(t4, fileManager, interchangeMetaManagers);
        }
    }
}