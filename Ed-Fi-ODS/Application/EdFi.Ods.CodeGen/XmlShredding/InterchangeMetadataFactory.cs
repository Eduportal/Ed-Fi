namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System.Collections.Generic;
    using System.Linq;

    using EdFi.Ods.CodeGen.XsdToWebApi;
    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    public class InterchangeMetadataFactory : ICanProduceInterchangeMetadata
    {
        private List<IManageEntityMetadata> _allAggregates;

        public InterchangeMetadataFactory()
        {
            this._allAggregates = new List<IManageEntityMetadata>();
        }

        public IEnumerable<IInterchangeMetadata> Generate(IEnumerable<string> xsdFilePaths)
        {
            var allMetadata = new List<IInterchangeMetadata>();
            foreach (var xsdFilePath in xsdFilePaths)
            {
                var parsedResults = InterchangeLoader.Load(xsdFilePath);
                if (!parsedResults.Any()) continue;
                var parsedResult = parsedResults.Single();
                var metaData = new InterchangeMetadata(parsedResult.XmlSchemaObjectName.Replace("Interchange", ""));
                var aggregateRoots = this.FilterOutDuplicateAggreatesAndReturnEntityMetadataManagers(parsedResult.GetChildElementsToBeParsed());
                aggregateRoots.ForEach(metaData.AddAggregateMetadataMgr);
                allMetadata.Add(metaData);
            }
            return allMetadata;
        }

        private List<IManageEntityMetadata> FilterOutDuplicateAggreatesAndReturnEntityMetadataManagers(IEnumerable<ParsedSchemaObject> parsedSchemaObjects)
        {
            var managers = new List<IManageEntityMetadata>();
            foreach (var parsedSchemaObject in parsedSchemaObjects)
            {
                var manager = new EntityMetadataManager(parsedSchemaObject, new XPathMapBuilder());
                if(this._allAggregates.Any(a => a.EntityName == manager.EntityName)) continue;
                this._allAggregates.Add(manager);
                managers.Add(manager);
            }
            return managers;
        }
    }
}