namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System;
    using System.IO;
    using System.Linq;

    public class InterchangeXsdFileProvider : IInterchangeXsdFileProvider
    {
        public const string SearchPattern = "*Interchange-*.xsd";

        private readonly string schemaDir;

        public InterchangeXsdFileProvider(string schemaDir)
        {
            this.schemaDir = schemaDir;
        }

        public string[] GetInterchangeFilePaths()
        {
            var files = Directory.GetFiles(schemaDir, SearchPattern).ToArray();

            if (!files.Any())
                throw new Exception(string.Format(
                    "Did not find any XSD files to use when creating XML-to-Resource Factory objects.  Please make certain the XSD files exist in the {0} folder.", schemaDir));
            return files;
        }
    }
}