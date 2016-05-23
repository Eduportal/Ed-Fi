using EdFi.Common.Extensions;
using EdFi.TestObjects;
using EdFi.TestObjects.Builders;

namespace EdFi.Ods.Tests.TestObjects.CustomBuilders
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;

    using global::EdFi.Ods.Common.Utils.Extensions;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    public class FixedLengthStringValueBuilderTests
    {
        private class TestModel
        {
            public string DontHandleThisOne { get; set; }

            public int SomeIntProperty { get; set; }

            [StringLength(20)]
            public string MaxLengthTwenty { get; set; }

            [StringLength(1)]
            public string MaxLengthOne { get; set; }
        }

        [TestFixture]
        public class When_property_is_not_a_string : TestBase
        {
            private ValueBuildResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var builder = new FixedLengthStringValueBuilder();
                var containingType = typeof (TestModel);
                Expression<Func<TestModel, object>> propertyExpression = x => x.SomeIntProperty;
                var propertyName = propertyExpression.MemberName();
                var propertyInfo = containingType.GetProperties().Single(x => x.Name == propertyName);
                var propertyType = propertyInfo.PropertyType;

                this._result = builder.TryBuild(new BuildContext(propertyName, propertyType, null, containingType, null, BuildMode.Create));
            }

            [Test]
            public void Should_not_handle_this_value()
            {
                this._result.Handled.ShouldBeFalse();
            } 
        }

        [TestFixture]
        public class When_property_has_no_attribute : TestBase
        {
            private ValueBuildResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var builder = new FixedLengthStringValueBuilder();
                var factory = new TestObjectFactory(new[] {builder}, null, new CustomAttributeProvider());

                var containingType = typeof (TestModel);
                Expression<Func<TestModel, string>> propertyExpression = x => x.DontHandleThisOne;
                var propertyName = propertyExpression.MemberName();
                var propertyInfo = containingType.GetProperties().Single(x => x.Name == propertyName);
                var propertyType = propertyInfo.PropertyType;

                this._result = builder.TryBuild(new BuildContext(propertyName, propertyType, null, containingType, null, BuildMode.Create));
            }

            [Test]
            public void Should_not_handle_this_value()
            {
                this._result.Handled.ShouldBeFalse();
            } 
        }

        [TestFixture]
        public class When_there_is_no_parent_type : TestBase
        {
            private ValueBuildResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var builder = new FixedLengthStringValueBuilder();

                this._result = builder.TryBuild(new BuildContext(string.Empty, typeof(string), null, null, null, BuildMode.Create));
            }

            [Test]
            public void Should_not_handle_this_value()
            {
                this._result.Handled.ShouldBeFalse();
            } 
        }

        [TestFixture]
        public class When_property_is_attributed_with_a_string_length_validation_of_max_length_20 : TestBase
        {
            private ValueBuildResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var builder = new FixedLengthStringValueBuilder();
                var factory = new TestObjectFactory(new[] { builder }, null, new CustomAttributeProvider());

                var parentType = typeof(TestModel);
                Expression<Func<TestModel, string>> propertyExpression = x => x.MaxLengthTwenty;
                var propertyName = propertyExpression.MemberName();
                var propertyInfo = parentType.GetProperties().Single(x => x.Name == propertyName);
                var propertyType = propertyInfo.PropertyType;

                builder.Reset();

                this._result = builder.TryBuild(new BuildContext(propertyName, propertyType, null, parentType, null, BuildMode.Create));
            }

            [Test]
            public void Should_handle_this_value()
            {
                this._result.Handled.ShouldBeTrue();
            }

            [Test]
            public void Should_fill_value_with_a_string_of_length_20()
            {
                var expected = "0-String-0xxxxxxxxxx";
                
                //Sanity Check
                expected.Length.ShouldEqual(20);

                this._result.Value.ShouldEqual(expected);
            }
        }

        [TestFixture]
        public class When_property_is_attributed_with_a_string_length_validation_of_max_length_1 : TestBase
        {
            private ValueBuildResult _result_1;
            private ValueBuildResult _result_2;
            private ValueBuildResult _result_50;

            [TestFixtureSetUp]
            public void Setup()
            {
                var builder = new FixedLengthStringValueBuilder();
                var factory = new TestObjectFactory(new[] { builder }, null, new CustomAttributeProvider());

                var containingType = typeof(TestModel);
                Expression<Func<TestModel, string>> propertyExpression = x => x.MaxLengthOne;
                var propertyName = propertyExpression.MemberName();
                var propertyInfo = containingType.GetProperties().Single(x => x.Name == propertyName);
                var propertyType = propertyInfo.PropertyType;

                builder.Reset();
                
                this._result_1 = builder.TryBuild(new BuildContext(propertyName, propertyType, null, containingType, null, BuildMode.Create));
                this._result_2 = builder.TryBuild(new BuildContext(propertyName, propertyType, null, containingType, null, BuildMode.Create));
                for(int ii = 3; ii <50; ii++)
                    builder.TryBuild(new BuildContext(propertyName, propertyType, null, containingType, null, BuildMode.Create));
                this._result_50 = builder.TryBuild(new BuildContext(propertyName, propertyType, null, containingType, null, BuildMode.Create));
            }

            [Test]
            public void Should_handle_this_value()
            {
                this._result_1.Handled.ShouldBeTrue();
            }

            [Test]
            public void Should_fill_value_with_a_string_of_length_1()
            {
                this._result_1.Value.ShouldEqual("0");
            }

            [Test]
            public void Should_fill_subsequent_values_with_unique_values()
            {
                this._result_2.Value.ShouldEqual("1");
                this._result_50.Value.ShouldEqual("n");
            }
        }
    }
}