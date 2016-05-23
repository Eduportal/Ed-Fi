using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using EdFi.Common.Extensions;
using EdFi.Ods.BulkLoad.Core.Dictionaries;
using EdFi.Ods.XmlShredding;

namespace EdFi.Ods.BulkLoad.Core
{
    public class IndexedXmlFileReader : IIndexedXmlFileReader
    {
        private readonly IStreamBuilder _streamBuilder;
        private readonly Func<string, string, IXmlGPS> _gpsFactoryMethod;
        private readonly IDbDictionary xmlNodes;
        private XmlNamespaceManager _namespaceManager;

        // TODO: make this a configurable setting
        private const string NAMESPACEPREFIX = "EXTENSION";

        private void InitXmlCacheFromFile(string interchangeFileName)
        {

            using (var sr = new StreamReader(_streamBuilder.Build(interchangeFileName), true))
            {
                // Create the XmlReader from the StreamReader
                var xr = XmlReader.Create(sr);
                _namespaceManager = new XmlNamespaceManager(xr.NameTable);
                
                // TODO: make this a configurable setting
                _namespaceManager.AddNamespace(NAMESPACEPREFIX, "http://ed-fi.org/0200");

                // Read the root element's start tag (so we can iterate through the top-level elements of the document)
                xr.ReadStartElement();
                
                while (xr.IsStartElement())  //while we have a content node (calling IsStartElement results in a call to MoveToContent while skipping ignored types)
                {
                    if (xr.NodeType != XmlNodeType.Element) continue; //if it isn't an element move on

                    // Read the element anme
                    var elementName = xr.Name;

                    // Read the entire element from the reader into an XElement
                    var element = (XElement)XNode.ReadFrom(xr);

                    xmlNodes.Add(elementName, element);
                }   
            }
        }

        public IndexedXmlFileReader(string interchangeFileName, IStreamBuilder streamBuilder, Func<string,string,IXmlGPS> gpsFactoryMethod, IDbDictionary dbDictionary)
        {
            xmlNodes = dbDictionary;
            _streamBuilder = streamBuilder;
            _gpsFactoryMethod = gpsFactoryMethod;
            InitXmlCacheFromFile(interchangeFileName);
        }

        public IEnumerable<XElement> GetNodesByEntity(string entity)
        {
            return xmlNodes.GetByAggregateName(entity);
        }

        public XElement FindForeignKeyElement(XElement startingElement, string serializedRefMap)
        {
            var gps = _gpsFactoryMethod(serializedRefMap, NAMESPACEPREFIX);
            return gps == null? null : RecurseMapOnReferenceElement(startingElement, gps);
        }

        private XElement RecurseMapOnReferenceElement(XElement element, IXmlGPS map)
        {
            var currentElement = element.Name.LocalName.EqualsIgnoreCase(map.GetRawPathToCurrent()) ? element : element.XPathSelectElement(map.GetXPathToCurrent(), _namespaceManager);
            if (currentElement == null) return null;
            if (map.CurrentStepIsTerminal()) return currentElement;
            string refId = currentElement.AttributeValue("ref");
            if (refId != null)
            {
                currentElement = GetElementById(map.GetReferencedAggregateTypes(), refId);
                if(currentElement == null) return null;
                map.ReferenceWasToAggregate(currentElement.Name.LocalName);
            }
            map.GoToNextStep();
            return map.CurrentStepIsAReference() ? RecurseMapOnReferenceElement(currentElement, map) : currentElement.XPathSelectElement(map.GetXPathToCurrent(), _namespaceManager);
        }

        private XElement GetElementById(IEnumerable<string> searchEntities, string id)
        {
            return xmlNodes.GetByAggregateNameAndId(searchEntities, id);
        }

        public void Dispose()
        {
            xmlNodes.Dispose();
        }
    }
}