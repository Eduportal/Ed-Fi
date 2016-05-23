using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using EdFi.Ods.CodeGen;

namespace EdFi.Ods.Tests.Metadata 
{
    public static class TablesByAggregateRoot
    {
        private static Dictionary<string, string[]> _tablesByType;
        private static readonly object _lock = new object();

        public static string[] TablesFor(string aggregateType)
        {
            lock (_lock)
            {
                if (_tablesByType == null)
                    InitTablesFromXml();
            }

            if (!_tablesByType.ContainsKey(aggregateType))
                return new[] {aggregateType};

            var reversedSinceTheseSeemToBeEmittedStartingAtTheRoot = _tablesByType[aggregateType].Reverse();
            return reversedSinceTheseSeemToBeEmittedStartingAtTheRoot.ToArray();
        }

        private static void InitTablesFromXml()
        {
            _tablesByType = new Dictionary<string, string[]>();

            Type markerType = typeof (DomainModelFactory);
            
            using (var stream = markerType.Assembly.GetManifestResourceStream("EdFi.Ods.CodeGen.App_Packages.Ed_Fi.Metadata.DomainMetadata.xml"))
            using (var reader = new StreamReader(stream))
            {
                var xml = reader.ReadToEnd();
                var xdoc = XDocument.Parse(xml);
                var aggregateCollection = xdoc.Descendants("Aggregates");
                var aggregates = aggregateCollection.Descendants("Aggregate").ToArray();
                foreach (var aggregate in aggregates)
                {
                    var aggregateName = aggregate.Attribute("root").Value;
                    var entities =
                        (from e in aggregate.Descendants("Entity")
                         let table = e.GetAttributeValue("table")
                         let schema = e.GetAttributeValue("schema", "edfi")
                         select string.Format("[{0}].[{1}]", schema, table));

                    var baseEntities =
                        (from e in aggregate.Descendants("Entity")
                         let table = e.GetAttributeValue("table")
                         let isA = e.GetAttributeValue("isA")
                         select string.Format("[{0}].[{1}]", "edfi", isA));

                    _tablesByType[aggregateName] = entities.Concat(baseEntities).ToArray();
                }
            }
        }
    }

    static class XElementExtensions
    {
        public static string GetAttributeValue(this XElement element, XName attributeName, string defaultValue = null)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                return defaultValue;

            return attribute.Value;
        }
    }
}