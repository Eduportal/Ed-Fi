namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public interface IMetadata
    {
        void Load(string metaDataXmlFile);
        IEnumerable<string> GetConcreteAggregates();
        bool IsAggregateRoot(string entityName);
    }

    public class Metadata : IMetadata
    {
        private IEnumerable<string> aggregates;

        public void Load(string metaDataXmlFile)
        {
            var domainMetadataDoc = XDocument.Load(metaDataXmlFile);

            this.aggregates = domainMetadataDoc
                .Descendants("Aggregate")
                .Where(x => x.Descendants("Entity").Any(e => e.AttributeValue("table") ==  x.AttributeValue("root") &&
                                                             e.AttributeValue("isAbstract") != "true"))
                .Select(x => x.AttributeValue("root"))
                .ToList();
        }

        public IEnumerable<string> GetConcreteAggregates()
        {
            return this.aggregates.Where(x => x.EndsWith("Descriptor"));
        }

        public bool IsAggregateRoot(string entityName)
        {
            return this.aggregates.Any(x => x == entityName);
        }
    }
   
}