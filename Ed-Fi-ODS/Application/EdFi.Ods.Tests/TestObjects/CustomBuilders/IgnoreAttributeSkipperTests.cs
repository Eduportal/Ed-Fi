using EdFi.TestObjects;
using EdFi.TestObjects.Builders;

namespace EdFi.Ods.Tests.TestObjects.CustomBuilders
{
    using System;

    using NUnit.Framework;

    using Should;

    public class IgnoreAttributeSkipperTests
    {
        public class MyCustomIgnoreThingyAttribute : Attribute
        {
        }

        public class SomeOtherAttribute : Attribute
        {
        }

        public class ExampleClass
        {
            [MyCustomIgnoreThingy]
            public string Foo { get; set; }

            [SomeOther]
            public string Bar { get; set; }
        }

        [TestFixture]
        public class When_property_is_attributed_with_something_that_contains_the_word_ignore
        {
            private ValueBuildResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var builder = new IgnoreAttributeSkipper();
                var factory = new TestObjectFactory(new[] { builder }, null, new CustomAttributeProvider());

                this._result = builder.TryBuild(new BuildContext("Foo", typeof(string), null, typeof(ExampleClass), null, BuildMode.Create));
            }

            [Test]
            public void Should_be_ignored()
            {
                this._result.Handled.ShouldBeTrue();
                this._result.ShouldSkip.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_property_is_not_attributed_with_something_that_contains_the_word_ignore
        {
            private ValueBuildResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var builder = new IgnoreAttributeSkipper();
                var factory = new TestObjectFactory(new[] { builder }, null, new CustomAttributeProvider());

                this._result = builder.TryBuild(new BuildContext("Bar", typeof (string), null, typeof (ExampleClass), null, BuildMode.Create));
            }

            [Test]
            public void Should_not_be_handled_or_skipped()
            {
                this._result.Handled.ShouldBeFalse();
                this._result.ShouldSkip.ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_parent_type_is_null
        {
            private ValueBuildResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var builder = new IgnoreAttributeSkipper();

                this._result = builder.TryBuild(new BuildContext("Bar", typeof (string), null, null, null, BuildMode.Create));
            }

            [Test]
            public void Should_not_be_handled_or_skipped()
            {
                this._result.Handled.ShouldBeFalse();
                this._result.ShouldSkip.ShouldBeFalse();
            }
        }
    }
}