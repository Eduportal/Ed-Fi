namespace EdFi.Ods.Tests.EdFi.Common.Context
{
    using System.Web;

    using global::EdFi.Common.Context;

    using NUnit.Framework;

    using Should;

    using Subtext.TestLibrary;

    [TestFixture]
    public class HttpContextStorageTests
    {
        [Test]
        public void Should_SetValue_in_HttpContext()
        {
            using (var simulatedContext = new HttpSimulator())
            {
                var key = "testKey";
                var expected = "Some Text";
                simulatedContext.SimulateRequest();
                var contextStorage = new HttpContextStorage();
                contextStorage.SetValue(key, expected);
                var actual = HttpContext.Current.Items[key];
                actual.ShouldEqual(expected);
            }
        }

        protected class TestObject
        {
            public string Text { get; set; }
        }

        [Test]
        public void Should_Get_Generic_Value_in_HttpContext()
        {
            using (var simulatedContext = new HttpSimulator())
            {
                var key = "testKey";
                var expected = "Some Text";
                simulatedContext.SimulateRequest();
                var contextStorage = new HttpContextStorage();
                var testObject = new TestObject {Text = expected};
                contextStorage.SetValue(key, testObject);
                var actual = contextStorage.GetValue<TestObject>(key);
                actual.Text.ShouldEqual(expected);
            }
        }
    }
}
