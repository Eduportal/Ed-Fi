using Should;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using Newtonsoft.Json;
using UnitTests._Bases;

namespace UnitTests.EdFi.Ods.BulkLoad.XmlMapBuilderTests
{
    [TestFixture]
    public class When_Serializing_A_String_With_Special_Chars
    {
        [Test]
        public void Should_Serialize_And_Deserialize()
        {
            var expected = new XmlMapBuilderTestBase.RawStep {
                ElementName = "ProgramReference", 
                IsEdOrgRef = false,
                ParentElement = @"a_serialized""/\_map",
                IsReference = true
            };

            var serializedrawStep = JsonConvert.SerializeObject(expected);

            var actual = JsonConvert.DeserializeObject<XmlMapBuilderTestBase.RawStep>(serializedrawStep);

            var compare = new CompareObjects();
            compare.Compare(actual, expected).ShouldBeTrue();
        }
    }
}
