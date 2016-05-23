using EdFi.Common.Extensions;
using EdFi.Ods.Common.Utils.Extensions;
using EdFi.TestObjects;
using EdFi.TestObjects.Builders;
using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.TestObjects.CustomBuilders
{
    public class UpdatePropertySkipperTests
    {
        private class Ignore1 : Attribute{}
        private class Ignore2 : Attribute{}

        private class TestModel
        {
            [Ignore1]
            public string DontHandleThisOne { get; set; }
            [Ignore2]
            public string DontHandleThisTwo { get; set; }

            public int SomeIntProperty { get; set; }
        }

        [TestFixture]
        public class When_property_has_been_ignored_and_we_are_modifying
        {
            private ValueBuildResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                Expression<Func<TestModel, object>> propertyExpression = x => x.DontHandleThisOne;

                UpdatePropertySkipper.ClearIgnores();
                UpdatePropertySkipper.IgnorePropertiesMarkedWith<Ignore1>();
                UpdatePropertySkipper.IgnorePropertiesMarkedWith<Ignore2>();
                var builder = new UpdatePropertySkipper();

                var containingType = typeof(TestModel);
                var propertyName = propertyExpression.MemberName();
                var propertyInfo = containingType.GetProperties().Single(x => x.Name == propertyName);
                var propertyType = propertyInfo.PropertyType;

                builder.Reset();
                var factory = new TestObjectFactory(new[] { builder }, null, new CustomAttributeProvider());

                this._result = builder.TryBuild(new BuildContext(propertyName, propertyType, null, containingType, null, BuildMode.Modify));
            }

            [Test]
            public void Should_handle_property()
            {
                this._result.Handled.ShouldBeTrue();
            }

            [Test]
            public void Should_indicate_property_is_ignored()
            {
                this._result.ShouldSkip.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_additional_property_has_been_ignored_and_we_are_modifying
        {
            private ValueBuildResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                Expression<Func<TestModel, object>> propertyExpression = x => x.DontHandleThisTwo;

                UpdatePropertySkipper.ClearIgnores();
                UpdatePropertySkipper.IgnorePropertiesMarkedWith<Ignore1>();
                UpdatePropertySkipper.IgnorePropertiesMarkedWith<Ignore2>();
                var builder = new UpdatePropertySkipper();

                var containingType = typeof(TestModel);
                var propertyName = propertyExpression.MemberName();
                var propertyInfo = containingType.GetProperties().Single(x => x.Name == propertyName);
                var propertyType = propertyInfo.PropertyType;

                builder.Reset();
                var factory = new TestObjectFactory(new[] { builder }, null, new CustomAttributeProvider());

                this._result = builder.TryBuild(new BuildContext(propertyName, propertyType, null, containingType, null, BuildMode.Modify));
            }

            [Test]
            public void Should_handle_property()
            {
                this._result.Handled.ShouldBeTrue();
            }

            [Test]
            public void Should_indicate_property_is_ignored()
            {
                this._result.ShouldSkip.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_property_has_been_ignored_and_we_are_creating
        {
            private ValueBuildResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                Expression<Func<TestModel, object>> propertyExpression = x => x.DontHandleThisOne;

                UpdatePropertySkipper.ClearIgnores();
                UpdatePropertySkipper.IgnorePropertiesMarkedWith<Ignore1>();
                UpdatePropertySkipper.IgnorePropertiesMarkedWith<Ignore2>();
                var builder = new UpdatePropertySkipper();

                var containingType = typeof(TestModel);
                var propertyName = propertyExpression.MemberName();
                var propertyInfo = containingType.GetProperties().Single(x => x.Name == propertyName);
                var propertyType = propertyInfo.PropertyType;

                builder.Reset();
                this._result = builder.TryBuild(new BuildContext(propertyName, propertyType, null, containingType, null, BuildMode.Create));
            }

            [Test]
            public void Should_not_handle_property()
            {
                this._result.Handled.ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_property_has_not_been_ignored_by_name_and_we_are_modifying
        {
            private ValueBuildResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                Expression<Func<TestModel, int>> propertyExpression = x => x.SomeIntProperty;

                UpdatePropertySkipper.ClearIgnores();
                UpdatePropertySkipper.IgnorePropertiesMarkedWith<Ignore1>();
                UpdatePropertySkipper.IgnorePropertiesMarkedWith<Ignore2>();
                var builder = new UpdatePropertySkipper();

                var containingType = typeof(TestModel);
                var propertyName = propertyExpression.MemberName();
                var propertyInfo = containingType.GetProperties().Single(x => x.Name == propertyName);
                var propertyType = propertyInfo.PropertyType;

                builder.Reset();
                var factory = new TestObjectFactory(new[] { builder }, null, new CustomAttributeProvider());

                this._result = builder.TryBuild(new BuildContext(propertyName, propertyType, null, containingType, null, BuildMode.Modify));
            }

            [Test]
            public void Should_not_handle_property()
            {
                this._result.Handled.ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_parent_type_is_not_set
        {
            private ValueBuildResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                Expression<Func<TestModel, int>> propertyExpression = x => x.SomeIntProperty;

                UpdatePropertySkipper.ClearIgnores();
                UpdatePropertySkipper.IgnorePropertiesMarkedWith<Ignore1>();
                UpdatePropertySkipper.IgnorePropertiesMarkedWith<Ignore2>();
                var builder = new UpdatePropertySkipper();

                Type parentTypeNullOnPurpose = null;
                var propertyType = typeof(string);

                builder.Reset();
                this._result = builder.TryBuild(new BuildContext("DontHandleThisOne", propertyType, null, parentTypeNullOnPurpose, null, BuildMode.Modify));
            }

            [Test]
            public void Should_not_handle_property()
            {
                this._result.Handled.ShouldBeFalse();
            }
        }
    }
}