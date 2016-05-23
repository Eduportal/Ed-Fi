namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    using global::EdFi.Ods.BulkLoad.Core;

    public class TestIndexedFile : IIndexedXmlFileReader
    {
        private readonly string _xmlElements;
        private readonly int _numberOfAggregatesToContain;

        public TestIndexedFile(int numberOfAggregatesToContain = 0)
        {
            this._numberOfAggregatesToContain = numberOfAggregatesToContain;
        }
        
        public TestIndexedFile(string xmlElements)
        {
            this._xmlElements = xmlElements;
        }
        
        IEnumerable<XElement> IIndexedXmlFileReader.GetNodesByEntity(string entity)
        {
            return string.IsNullOrWhiteSpace(this._xmlElements)
                ? Enumerable.Range(0, this._numberOfAggregatesToContain)
                    .Select(x => new XElement(string.Format("node{0}", x)))
                : XElement.Parse(string.Format("<root>{0}</root>", this._xmlElements)).Elements();
        }

        public XElement GetElementById(IEnumerable<string> searchEntities, string id)
        {
            throw new System.NotImplementedException();
        }

        public XElement FindElement(IEnumerable<string> searchEntities, Func<XElement, bool> searchCriteria)
        {
            throw new NotImplementedException();
        }

        public XElement FindForeignKeyElement(XElement startingElement, string serializedRefMap)
        {
            throw new NotImplementedException();
        }

        public XElement FindByPath(XElement containingElement, string xPath)
        {
            return null;
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}