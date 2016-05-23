namespace EdFi.Ods.Tests.EdFi.Ods.XmlShredding
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Xml.Linq;

    using global::EdFi.Ods.Api.Models.Resources.DisciplineDescriptor;
    using global::EdFi.Ods.Api.Models.Resources.LevelDescriptor;
    using global::EdFi.Ods.Tests._Bases;
    using global::EdFi.Ods.XmlShredding;

    using KellermanSoftware.CompareNetObjects;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class When_shredding_descriptors_xml_file : TestFixtureBase
    {
        private string filePath;
        private IEnumerable<object> generatedEntities;

        protected override void EstablishContext()
        {
            var codeBasePath = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path;
            var rootPath = Path.GetDirectoryName(codeBasePath);
            this.filePath = Path.Combine(rootPath, @"EdFi.Ods.XmlShredding\Xml\Interchange-Descriptors.xml");

        }


        public static object CreateInstance(Type type)
        {
            var constructorInfo = type.GetConstructor(Type.EmptyTypes);
            var createDelegate = Expression.Lambda(Expression.New(constructorInfo)).Compile();
            return createDelegate.DynamicInvoke();
        }
        protected override void ExecuteBehavior()
        {
            var xmlDoc = XDocument.Load(this.filePath);
            var nodes = xmlDoc.Root.Elements();

            INodeSearch nodeSearch = null;

            this.generatedEntities = nodes
                .Select(xml => BuildResource(xml, nodeSearch))
                .ToList();
        }

        private static object BuildResource(XElement xmlNode, INodeSearch nodeSearch)
        {
            var factoryType = typeof(IResourceFactory<>).Assembly.GetExportedTypes().Single(t => t.Name == xmlNode.Name.LocalName + "Factory");

            var factory = CreateInstance(factoryType);

            var buildMethod = factoryType.GetMethod("Build");

            return buildMethod.Invoke(factory, new object[] { xmlNode, nodeSearch });

        }


        [Test]
        public void Should_generate_21_level_descriptors()
        {
            this.generatedEntities.OfType<LevelDescriptor>().Count().ShouldEqual(21);
        }

        [Test]
        public void Should_generate_12_discipline_descriptors()
        {
            this.generatedEntities.OfType<DisciplineDescriptor>().Count().ShouldEqual(12);
        }

        [Test]
        public void Verify_level_descriptor_code01_is_fully_populated()
        {
            var expected = new LevelDescriptor()
            {
                CodeValue = "01",
                ShortDescription = "Adult Education",
                Description = "Adult Education",
                EffectiveBeginDate = new DateTime(2012, 07, 1),
                Namespace = @"uri://ed-fi.org/Descriptors/Level/Adult-Education",
            };

            var actual = this.generatedEntities.OfType<LevelDescriptor>().Single(x => x.CodeValue == "01");

            var differences = Compare(expected, actual);

            differences.ShouldBeEmpty();
        }

        private static IEnumerable<Difference> Compare(LevelDescriptor expected, LevelDescriptor actual)
        {
            var comparer = new CompareObjects();

            comparer.Compare(expected, actual);

            var differences = comparer.Differences;

            foreach (var differnce in comparer.Differences)
            {
                Console.WriteLine("Property {0} diffrence, expected: {1}, actual: {2}", differnce.PropertyName,
                                  differnce.Object1Value, differnce.Object2Value);
            }
            return differences;
        }

        [Test]
        public void Verify_discipline_descriptor_code01_is_fully_populated()
        {
            var expected = new DisciplineDescriptor()
            {
                CodeValue = "01",
                ShortDescription = "Removal from Classroom",
                Description = "Removal from Classroom",
                EffectiveBeginDate = new DateTime(2012, 07, 1),
                Namespace = @"uri://ed-fi.org/Descriptors/Discipline/Removal-from-Classroom",
            };

            var actual = this.generatedEntities.OfType<DisciplineDescriptor>().Single(x => x.CodeValue == "01");
            var comparer = new CompareObjects();

            comparer.Compare(expected, actual);
            foreach (var differnce in comparer.Differences)
            {
                Console.WriteLine("Property {0} didn't match. Expected: {1}, actual: {2}", differnce.PropertyName, differnce.Object1Value, differnce.Object2Value);
            }

            comparer.Differences.ShouldBeEmpty();
        }
    }
}
