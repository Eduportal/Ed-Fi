using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Tests.ConventionTestClasses.Models;
using EdFi.Ods.Utilities.LoadGeneration.ValueBuilders;
using EdFi.TestObjects;
using NUnit.Framework;
using Rhino.Mocks;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ValueBuilders
{
    public class EdFiDescriptorValueBuilderFixtures
    {
        [Ignore("GKM - I have no idea what this is supposed to be testing.")]
        public class When_randomizing_an_item_with_a_descriptor : TestFixtureBase
        {
            private ITestObjectFactory objectFactory;
            private CompetencyObjectiveReference itemToRandomize;
            private const string CodeValue = "TestCodeValue";

            protected override void Arrange()
            {
                var descriptorFactory = Stub<IItemFromApiProvider>();
                descriptorFactory.Expect(ds => ds.GetNext(typeof(GradeLevelDescriptor))).Return(new GradeLevelDescriptor { codeValue = CodeValue });

                var builderFactory = new BuilderFactory();
                builderFactory.AddToFront(() => new EdFiDescriptorValueBuilder(new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider()), descriptorFactory));

                objectFactory = new TestObjectFactory(builderFactory.GetBuilders(), new SystemActivator(), new CustomAttributeProvider());

                itemToRandomize = new CompetencyObjectiveReference();
            }

            protected override void Act()
            {
                objectFactory.RandomizeValue(itemToRandomize, typeof(CompetencyObjectiveReference));
            }

            [Test]
            public void Should_use_codeValue_from_IDescriptorFactory()
            {

                Assert.That(itemToRandomize.objectiveGradeLevelDescriptor, Is.EqualTo(CodeValue));
            }
        }
    }
}
