namespace EdFi.Ods.Tests.EdFi.Common.Context
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Threading.Tasks;

    using global::EdFi.Common.Context;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class CallContextStorageTests
    {
        [TestFixture]
        public class When_accessing_context_in_the_same_thread
        {
            [Test]
            public void Should_SetValue_in_CallContext()
            {
                var key = "testKey";
                var expected = "Some Text";
                var contextStorage = new CallContextStorage();
                contextStorage.SetValue(key, expected);
                var actual = CallContext.GetData(key);
                actual.ShouldEqual(expected);
            }

            protected class TestObject
            {
                public string Text { get; set; }
            }

            [Test]
            public void Should_Get_Generic_Value_in_CallContext()
            {
                var key = "testKey";
                var expected = "Some Text";
                var contextStorage = new CallContextStorage();
                var testObject = new TestObject {Text = expected};
                contextStorage.SetValue(key, testObject);
                var actual = contextStorage.GetValue<TestObject>(key);
                actual.Text.ShouldEqual(expected);
            }
        }

        [TestFixture]
        public class When_accessing_context_across_a_task_boundary
        {
            [Test]
            public void Should_retrieve_value()
            {
                new CallContextStorage().SetValue("MyReallyAwesomeKey", "My Cool Value");

                //Sanity Check
                var sameThreadValue = new CallContextStorage().GetValue<string>("MyReallyAwesomeKey");
                sameThreadValue.ShouldEqual("My Cool Value");
                Console.WriteLine("Same thread value works fine");

                var task = Task.Run(() => this.TestGetValue());
                task.Wait();
            }

            private void TestGetValue()
            {
                var value = new CallContextStorage().GetValue<string>("MyReallyAwesomeKey");
                value.ShouldEqual("My Cool Value");
            }
        }
    }
}