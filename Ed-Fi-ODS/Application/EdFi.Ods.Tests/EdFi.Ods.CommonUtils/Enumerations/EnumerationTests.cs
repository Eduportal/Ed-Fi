namespace EdFi.Ods.Tests.EdFi.Ods.CommonUtils.Enumerations
{
    using global::EdFi.Common;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    public class EnumerationTests
    {
        [TestFixture]
        public class When_enumeration_has_values : TestBase
        {
            private TestEnumeration[] _results;

            private class TestEnumeration : Enumeration<TestEnumeration, string>
            {
                public string MyId { get; private set; }
                public override string Id { get { return this.MyId; } }
                public string Name { get; private set; }

                public static TestEnumeration One = new TestEnumeration("1", "One");
                public static TestEnumeration Two = new TestEnumeration("2", "Two");

                private static TestEnumeration Problematic = new TestEnumeration("3", "I Cause Problems");

                public static string Detractor = "foo";

                public TestEnumeration(string id, string name)
                {
                    this.MyId = id;
                    this.Name = name;
                }
            }

            [TestFixtureSetUp]
            public void Setup()
            {
                this._results = TestEnumeration.GetValues();
            }

            [Test]
            public void Should_retrieve_all_values()
            {
                this._results.Length.ShouldEqual(2);
                this._results.ShouldContain(TestEnumeration.One);
                this._results.ShouldContain(TestEnumeration.Two);
            }

            [Test]
            public void Should_retrieve_value_by_id()
            {
                TestEnumeration.GetById("1").ShouldEqual(TestEnumeration.One);
            }

            [Test]
            public void Should_return_null_when_searching_by_nonexistent_id()
            {
                TestEnumeration.GetById("foo").ShouldBeNull();
            }
        }

        [TestFixture]
        public class When_enumeration_has_value_with_null_id : TestBase
        {
            private TestEnumeration[] _results;

            private class TestEnumeration : Enumeration<TestEnumeration, string>
            {
                public string MyId { get; private set; }
                public override string Id { get { return this.MyId; } }
                public string Name { get; private set; }

                public static TestEnumeration Null = new TestEnumeration(null, "Null");
                public static TestEnumeration One = new TestEnumeration("1", "One");
                public static TestEnumeration Two = new TestEnumeration("2", "Two");

                private static TestEnumeration Problematic = new TestEnumeration("3", "I Cause Problems");

                public static string Detractor = "foo";

                public TestEnumeration(string id, string name)
                {
                    this.MyId = id;
                    this.Name = name;
                }
            }

            [TestFixtureSetUp]
            public void Setup()
            {
                this._results = TestEnumeration.GetValues();
            }

            [Test]
            public void Should_retrieve_all_values()
            {
                this._results.Length.ShouldEqual(3);
                this._results.ShouldContain(TestEnumeration.Null);
                this._results.ShouldContain(TestEnumeration.One);
                this._results.ShouldContain(TestEnumeration.Two);
            }

            [Test]
            public void Should_retrieve_value_by_id()
            {
                TestEnumeration.GetById("1").ShouldEqual(TestEnumeration.One);
            }

            [Test]
            public void Should_retrive_value_with_null_id()
            {
                TestEnumeration.GetById(null).ShouldEqual(TestEnumeration.Null);
            }

            [Test]
            public void Should_return_null_when_searching_by_nonexistent_id()
            {
                TestEnumeration.GetById("foo").ShouldBeNull();
            }
        }

        [TestFixture]
        public class When_ids_are_equal : TestBase
        {
            private class EqualsEnumeration : Enumeration<EqualsEnumeration, int>
            {
                public override int Id
                {
                    get { return this.MyId; }
                }

                public int MyId { get; private set; }
                public string Name { get; private set; }

                public static EqualsEnumeration One = new EqualsEnumeration(1, "One");
                public static EqualsEnumeration Two = new EqualsEnumeration(1, "Two");

                public EqualsEnumeration(int id, string name)
                {
                    this.MyId = id;
                    this.Name = name;
                }
            }

            [Test]
            public void Should_be_equal()
            {
                EqualsEnumeration.One.ShouldEqual(EqualsEnumeration.Two);
            }

            [Test]
            public void Should_throw_exception_when_searching_by_duplicated_id()
            {
                try
                {
                    EqualsEnumeration.GetById(1);
                }
                catch
                {
                    Assert.Pass("Nothing to see here.... These are not the tests you're looking for.");
                }
                Assert.Fail("Should have thrown an exception");
            }
        }

        [TestFixture]
        public class When_ids_are_not_equal : TestBase
        {
            private class EqualsEnumeration : Enumeration<EqualsEnumeration, int>
            {
                public override int Id
                {
                    get { return this.MyId; }
                }

                public int MyId { get; private set; }
                public string Name { get; private set; }

                public static EqualsEnumeration One = new EqualsEnumeration(1, "One");
                public static EqualsEnumeration Two = new EqualsEnumeration(2, "Two");

                public EqualsEnumeration(int id, string name)
                {
                    this.MyId = id;
                    this.Name = name;
                }
            }

            [Test]
            public void Should_not_be_equal()
            {
                EqualsEnumeration.One.ShouldNotEqual(EqualsEnumeration.Two);
            }
        }
    }
}