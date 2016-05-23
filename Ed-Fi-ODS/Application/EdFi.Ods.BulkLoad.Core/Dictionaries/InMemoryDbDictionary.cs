using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using EdFi.Ods.XmlShredding;

namespace EdFi.Ods.BulkLoad.Core.Dictionaries
{
    public class InMemoryDbDictionary : IDbDictionary
    {
        private IDictionary<string, IList<XElement>> elementsByAggregateName = new Dictionary<string, IList<XElement>>();
        private IDictionary<string, XElement> elementsById = new Dictionary<string, XElement>();
 
        public void Add(string aggregateName, XElement element)
        {
            CheckDisposed();

            IList<XElement> elements;

            if (!elementsByAggregateName.TryGetValue(aggregateName, out elements))
            {
                elements = new List<XElement>();
                elementsByAggregateName[aggregateName] = elements;
            }

            AddElementById(element);

            elements.Add(element);

            // If the element contains any references that have id attributes, we need to add these to the cache
            var referencesWithId = from el in element.Descendants()
                                   where el.Attribute("id") != null && el.Name.LocalName.EndsWith("Reference")
                                   select el;
            
            foreach (var referenceWithId in referencesWithId)
            {
                AddElementById(referenceWithId);
            }
        }

        private void AddElementById(XElement element)
        {
            string id = element.AttributeValue("id");

            if (!string.IsNullOrEmpty(id))
                elementsById[id] = element;
        }

        public IList<XElement> GetByAggregateName(string aggregateName)
        {
            CheckDisposed();

            IList<XElement> elements;

            if (!elementsByAggregateName.TryGetValue(aggregateName, out elements))
            {
                elements = new List<XElement>();
                elementsByAggregateName[aggregateName] = elements;
            }

            return elements;
        }

        public XElement GetByAggregateNameAndId(IEnumerable<string> aggregateNames, string id)
        {
            CheckDisposed();

            XElement element;

            if (elementsById.TryGetValue(id, out element))
                return element;

            return null;
        }

        private bool disposed;

        private void CheckDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException("InMemoryDbDictionary");
        }

        public void Dispose()
        {
            disposed = true;
            elementsByAggregateName = null;
        }
    }
}