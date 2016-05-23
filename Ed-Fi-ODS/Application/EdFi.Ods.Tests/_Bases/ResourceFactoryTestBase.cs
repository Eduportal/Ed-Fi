namespace EdFi.Ods.Tests._Bases
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;

    using global::EdFi.Ods.BulkLoad.Core;
    using global::EdFi.Ods.BulkLoad.Core.Dictionaries;
    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.Entities.Common.Validation;
    using global::EdFi.Ods.Tests.TestObjects;
    using global::EdFi.Ods.XmlShredding;

    using NUnit.Framework;

    [TestFixture]
    public class ResourceFactoryTestBase
    {
        private IIndexedXmlFileReader reader;
        private IEnumerable<string> aggregateRootNames;

        [SetUp]
        public void Initialize()
        {
            this.reader = null;
            this.aggregateRootNames = null;
        }

        [TearDown]
        public void Dispose()
        {
            this.reader = null;
            this.aggregateRootNames = null;
        }

        [Test]
        public void DummyTest()
        {
            //This test enables the this test base fixture to appear as "Green" and not "Yellow" (Inconclusive) in resharper window, so as not to raise alarm
            //every time when running the tests, as to why some test is inconclusive
            Assert.Pass();
        }

        public void SetSource(Stream xmStream, IEnumerable<string> aggregateRootNmes)
        {
            this.reader = this.GetReaderFor(xmStream);
            this.aggregateRootNames = aggregateRootNmes;
        }

        private IIndexedXmlFileReader GetReaderFor(Stream xmlStream)
        {
            var gpsFunc = new Func<string, string, XmlGPS>((x, y) => new XmlGPS(new XPathMapBuilder().DeserializeMap(x), y));
            var builder = new TestStreamBuilder(() => xmlStream);
            return new IndexedXmlFileReader("Does not matter because test stream builder ignores it", builder, gpsFunc, new InMemoryDbDictionary());
        }

        private string GetResourceAndValidateReturnErrorMsg(XElement element, IIndexedXmlFileReader reader)
        {
            var validator = new DataAnnotationsObjectValidator();
            var factoryTypesList = typeof (IResourceFactory<>).Assembly.GetExportedTypes();
            var factoryType = factoryTypesList.SingleOrDefault(t => t.Name == element.Name.LocalName + "Factory");
            if (factoryType==null)
                throw new Exception(element.Name.LocalName + "Factory not found in ResourceFactories.generated file. Are you missing a core or extension xsd?");
            var constructorInfo = factoryType.GetConstructor(Type.EmptyTypes);
            var createDelegate = Expression.Lambda(Expression.New(constructorInfo)).Compile();
            var factory = createDelegate.DynamicInvoke();
            var buildMethod = factoryType.GetMethod("Build");
            try
            {
                var resource = buildMethod.Invoke(factory, new object[] {element, reader});
                var result = validator.ValidateObject(resource);
                if (result.All(r => string.IsNullOrWhiteSpace(r.ErrorMessage))) return string.Empty;

                var sb = new StringBuilder();
                var errors = result.Where(r => !string.IsNullOrWhiteSpace(r.ErrorMessage)).ToList();
                errors.ForEach(e => sb.AppendLine(string.Format("Reason: {0}", e)));
                return string.Format("{0} was not valid for the following reasons: {1}", element.Name, sb);
            }
            catch (Exception e)
            {
                var msg = e.InnerException != null ? e.InnerException.Message : e.Message;
                return string.Format("{0} threw ['{1}'] when the factory was invoked.  Here's the Xml: {2}",
                    element.Name, msg, element.Value);
            }
        }

        public int GetResourceCountInSource()
        {
            if(this.reader == null || this.aggregateRootNames == null) 
                throw new Exception("The test source was not set using SetSource(Stream source, IEnumerable<string> rootNames)");
            var rCount = 0;
            foreach (var aggregateRootName in this.aggregateRootNames)
            {
                this.reader.GetNodesByEntity(aggregateRootName).ToList().ForEach(r => rCount++);
            }
            return rCount;
        }

        public int GenerateResourcesAndReturnNumberValidFromSource()
        {
            if(this.reader == null || this.aggregateRootNames == null) 
                throw new Exception("The test source was not set using SetSource(Stream source, IEnumerable<string> rootNames)");
            var validResourceCount = 0;
            foreach (var aggregateRoot in this.aggregateRootNames)
            {
                var elementsInXml = this.reader.GetNodesByEntity(aggregateRoot).ToList();
                foreach (var xElement in elementsInXml)
                {
                    var errorMsg = this.GetResourceAndValidateReturnErrorMsg(xElement, this.reader);
                    if (string.IsNullOrWhiteSpace(errorMsg))
                    {
                        validResourceCount++;
                        continue;
                    }
                    Console.WriteLine(errorMsg);
                }
            }
            return validResourceCount;
        }

        public void ValidateXml(Stream xmlStream)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.CloseInput = true;
            settings.ValidationEventHandler += Handler;

            settings.ValidationType = ValidationType.Schema;
            var files = Directory.GetFiles("schemas");
            foreach (var xsd in files)
            {
                settings.Schemas.Add(null, xsd);
            }
            settings.ValidationFlags =
                 XmlSchemaValidationFlags.ReportValidationWarnings |
            XmlSchemaValidationFlags.ProcessIdentityConstraints |
            XmlSchemaValidationFlags.ProcessInlineSchema |
            XmlSchemaValidationFlags.ProcessSchemaLocation;

            using (XmlReader validatingReader = XmlReader.Create(xmlStream, settings))
            {
                while (validatingReader.Read()) { /* just loop through document */ }
            }
        }

        private static void Handler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error || e.Severity == XmlSeverityType.Warning)
            {
                Console.WriteLine("Line: {0}, Position: {1} \"{2}\"", e.Exception.LineNumber, e.Exception.LinePosition, e.Exception.Message);
                //Assert.Fail("Xml is invalid");
            }

        }
    }
}