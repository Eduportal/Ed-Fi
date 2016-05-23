using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.Tests.ConventionTestClasses.Models;
using EdFi.Ods.Utilities.LoadGeneration.ValueBuilders;
using EdFi.TestObjects;
using NUnit.Framework;
using Rhino.Mocks;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ValueBuilders
{
    public class EdFiTypeValueBuilderFixtures
    {
        public class When_Randomizing_An_Item_With_A_Type : TestFixtureBase
        {
            private TestObjectFactory objectFactory;
            private EducationContentAppropriateSex itemToRandomize;
            private const string shortDescription = "TestShortDescription";

            protected override void Arrange()
            {
                var descriptorFactory = MockRepository.GenerateMock<IItemFromApiProvider>();
                descriptorFactory.Expect(ds => ds.GetNext(typeof(SexType))).Return(new SexType { shortDescription = shortDescription });

                var builderFactory = new BuilderFactory();
                builderFactory.AddToFront(() => new EdFiTypeValueBuilder(new ApiSdkReflectionProvider(new TestApiSdkAssemblyProvider()), descriptorFactory));

                objectFactory = new TestObjectFactory(builderFactory.GetBuilders(), new SystemActivator(), new CustomAttributeProvider());

                itemToRandomize = new EducationContentAppropriateSex();
            }

            protected override void Act()
            {
                objectFactory.RandomizeValue(itemToRandomize, typeof(EducationContentAppropriateSex));
            }

            [Test]
            public void Should_Use_CodeValue_From_IDescriptorFactory()
            {
                Assert.That(itemToRandomize.sexType, Is.EqualTo(shortDescription));
            }
        }
    }
}
