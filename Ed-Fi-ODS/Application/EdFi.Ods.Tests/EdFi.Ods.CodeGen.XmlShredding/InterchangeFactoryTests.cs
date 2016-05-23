using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using EdFi.Ods.CodeGen.XmlShredding;
using NUnit.Framework;
using Should;
using EdFi.Ods.Tests.Metadata;

namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.XmlShredding
{
    [TestFixture]
    public class InterchangeFactoryTests
    {

        private const string SchemaDirectory = @"..\..\..\schema.codegen";

        [Test, Ignore("Test data is location specific")]
        public void When_Xsd_Exist_Should_Create_IInterchangeMetadata_For_Each_Xsd()
        {
            var files =
                Directory.GetFiles(SchemaDirectory)
                    .Where(f => f.EndsWith("Descriptors-Extension.xsd") || f.EndsWith("EducationOrganization.xsd"));
            var sut = new InterchangeMetadataFactory();
            var result = sut.Generate(files);
            result.Count(m => m.InterchangeName == "EducationOrganization").ShouldEqual(1);
            result.Count(m => m.InterchangeName == "Descriptors").ShouldEqual(1);
        }

        [Test]
        public void When_Xsd_Includes_Top_Level_Elements_That_Should_Be_Skipped_Should_Skip_Those_Elements()
        {

            var files = Directory.GetFiles(SchemaDirectory).Where(f => f.EndsWith("Standards.xsd") || f.EndsWith("Gradebook.xsd"));
            var sut = new InterchangeMetadataFactory();
            var aggreates = new List<IManageEntityMetadata>();
            foreach (var interchangeMetadata in sut.Generate(files))
            {
                aggreates.AddRange(interchangeMetadata.Aggregates);
            }
            aggreates.Count(a => a.EntityName == "SectionReference").ShouldEqual(0);
        }

        [Test]
        public void When_given_A_Non_interchange_xsd_should_ignore_it_gracefully_ie_no_exception()
        {
            var files = Directory.GetFiles(SchemaDirectory);
            var interchangeFiles = files.Count(f => f.Contains("Interchange-"));
            var sut = new InterchangeMetadataFactory();
            var result = sut.Generate(files);
            result.Count().ShouldEqual(interchangeFiles);
        }

        [Test, Ignore("Test data is location specific")]
        public void When_Loading_Descriptors_Should_Include_an_Aggregate_Metadata_for_each_Aggregate_Root()
        {
            var files =
                Directory.GetFiles(SchemaDirectory)
                    .Where(f => f.EndsWith("Descriptors-Extension.xsd")) ;
            var sut = new InterchangeMetadataFactory();
            var result = sut.Generate(files);

            Type markerType = typeof (TablesByAggregateRoot);
            var resourceName = string.Format("{0}.{1}", markerType.Namespace, "DomainMetadata.xml");
            using (var stream = markerType.Assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var xml = reader.ReadToEnd();
                var xdoc = XDocument.Parse(xml);
                var aggregates = xdoc 
                    .Descendants("Aggregate")
                    .Where(x => x.Descendants("Entity").Any(e => e.Attribute("table").Value ==  x.Attribute("root").Value &&
                                                             (e.Attribute("isAbstract") == null || e.Attribute("isAbstract") != null && e.Attribute("isAbstract").Value != "true")))
                    .Select(x => x.Attribute("root").Value)
                    .ToList();

                aggregates = aggregates.Where(x => x.EndsWith("Descriptor"))
                    .Where(x => x != "GraduationPlanTypeDescriptor")
                    .ToList();
                foreach (var aggregate in aggregates)
                {
                    result.First().Aggregates.Count(a => a.EntityName == aggregate).ShouldEqual(1, "Aggregate " + aggregate + " not found");
                }
            }
        }

        [Test]
        public void Given_Xsd_Files_That_Contain_The_Same_Aggregate_Root_Should_Only_Include_It_Once()
        {
            var files = Directory.GetFiles(SchemaDirectory).Where(f => f.EndsWith("Standards.xsd") || f.EndsWith("Gradebook.xsd"));
            var sut = new InterchangeMetadataFactory();
            var aggreates = new List<IManageEntityMetadata>();
            foreach (var interchangeMetadata in sut.Generate(files))
            {
                aggreates.AddRange(interchangeMetadata.Aggregates);
            }
            aggreates.Count(a => a.EntityName == "LearningObjective").ShouldEqual(1);
        }
    }
}