namespace EdFi.Ods.Tests.EdFi.Ods.XmlShredding
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    public class InterchangeFileManagerTests : TestFixtureBase
    {
        private IEnumerable<string> rootElements = new List<string>
            {
                "StateEducationAgency",
                "EducationServiceCenter",
                "LocalEducationAgency",
                "School",
                "Location",
                "ClassPeriod",
                "Course",
                "Program"
            };

        private string filePath;
        private InterchangeFileManager fileManager;
        private InterchangeFileIndex index;

        protected override void EstablishContext()
        {
            var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", string.Empty);
            filePath = Path.Combine(rootPath, @"EdFi.Ods.XmlShredding\Xml\Interchange-EducationOrganization.xml");
        }

        protected override void ExecuteTest()
        {
            index = new InterchangeFileIndex();
            fileManager = new InterchangeFileManager(index);
            fileManager.Load(filePath, rootElements);
        }

        [Test]
        public void Should_find_right_no_entity_types()
        {
            index.GetExistingEntities().Count().ShouldEqual(3);
        }

        [Test]
        public void Should_read_elements_and_parse_based_on_index()
        {
            var entities = index.GetExistingEntities();
            foreach (var entity in entities)
            {
                foreach (var node in fileManager.GetNodesByEntity(entity))
                {
                    var xelement = fileManager.Read(node);
                    xelement.Name.ShouldEqual(node.ElementName);
                }
            }
        }
    }
}
