using System;
using System.Linq;
using System.Linq.Expressions;
using EdFi.TestObjects.Builders;
using EdFi.TestObjects.Tests.Extensions;
using NUnit.Framework;
using Should;

namespace EdFi.TestObjects.Tests.Builders
{
    public class RangeConstrainedFloatingPointValueBuilderTests
    {
        private class TestModel
        {
            public decimal UnconstrainedDecimal { get; set; }

            [System.ComponentModel.DataAnnotations.Range(0, 1)]
            public int ConstrainedNonDecimal { get; set; }

            [System.ComponentModel.DataAnnotations.Range(0, 20)]
            public decimal ZeroToTwentyWithNoScaleSupplied { get; set; }

            [System.ComponentModel.DataAnnotations.Range(-1, 1)]
            public decimal NegativeOneToOneWithNoScaleSupplied { get; set; }

            [System.ComponentModel.DataAnnotations.Range(-999.99999, 999.99999)]
            public decimal PrecisionEightScaleFive { get; set; }

            [System.ComponentModel.DataAnnotations.Range(-.01, .01)]
            public decimal PrecisionTwoScaleTwo { get; set; }

            [System.ComponentModel.DataAnnotations.Range(0, 1)]
            public decimal? NullableDecimal { get; set; }
        }

        private static ValueBuildResult GetValueBuildResultForProperty(RangeConstrainedFloatingPointValueBuilder builder, Expression<Func<TestModel, object>> propertyExpression)
        {
            var containingType = typeof(TestModel);
            var propertyName = propertyExpression.MemberName();
            var propertyInfo = containingType.GetProperties().Single(x => x.Name == propertyName);
            var propertyType = propertyInfo.PropertyType;

            var result = builder.TryBuild(new BuildContext(propertyName, propertyType, null, containingType, null, BuildMode.Create));
            return result;
        }

        [TestFixture]
        public class When_property_is_a_nullable_decimal_with_a_Range_attribute //: TestFixtureBase
        {
            private ValueBuildResult result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var builder = new RangeConstrainedFloatingPointValueBuilder();
                var factory = new TestObjectFactory(new[] { builder }, null, new CustomAttributeProvider());

                this.result = GetValueBuildResultForProperty(builder, x => x.NullableDecimal);
            }

            [Test]
            public void Should_handle_nullable_decimals()
            {
                this.result.Handled.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_property_is_not_a_decimal_with_a_Range_attribute //: TestBase
        {
            private ValueBuildResult unconstrainedDecimalResult;
            private ValueBuildResult constrainedNonDecimalResult;

            [TestFixtureSetUp]
            public void Setup()
            {
                var builder = new RangeConstrainedFloatingPointValueBuilder();
                var factory = new TestObjectFactory(new[] {builder}, null, new CustomAttributeProvider());

                this.unconstrainedDecimalResult = GetValueBuildResultForProperty(builder, x => x.UnconstrainedDecimal);
                this.constrainedNonDecimalResult = GetValueBuildResultForProperty(builder, x => x.ConstrainedNonDecimal);
            }

            [Test]
            public void Should_not_handle_unconstrained_decimals()
            {
                this.unconstrainedDecimalResult.Handled.ShouldBeFalse();
            }

            [Test]
            public void Should_not_handle_constrained_non_decimals()
            {
                this.constrainedNonDecimalResult.Handled.ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_there_is_no_parent_type //: TestBase
        {
            private ValueBuildResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var builder = new RangeConstrainedFloatingPointValueBuilder();
                var factory = new TestObjectFactory(new[] { builder }, null, new CustomAttributeProvider());

                this._result = builder.TryBuild(new BuildContext(string.Empty, typeof(decimal), null, null, null, BuildMode.Create));
            }

            [Test]
            public void Should_not_handle_this_value()
            {
                this._result.Handled.ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_generating_two_values_for_a_decimal_property_with_no_scale_supplied_by_the_range //: TestFixtureBase
        {
            private ValueBuildResult firstBuildResult;
            private ValueBuildResult secondBuildResult;

            [TestFixtureSetUp]
            public void Act()
            {
                var builder = new RangeConstrainedFloatingPointValueBuilder();
                var factory = new TestObjectFactory(new[] { builder }, null, new CustomAttributeProvider());

                this.firstBuildResult = GetValueBuildResultForProperty(builder, x => x.NegativeOneToOneWithNoScaleSupplied);
                this.secondBuildResult = GetValueBuildResultForProperty(builder, x => x.NegativeOneToOneWithNoScaleSupplied);
            }

            [Test]
            public virtual void Should_handle_both_requests_indicating_the_value_should_be_set()
            {
                this.firstBuildResult.Handled.ShouldBeTrue("First request for value wasn't handled.");
                this.firstBuildResult.ShouldSetValue.ShouldBeTrue("First request didn't indicate that the value should be set.");

                this.secondBuildResult.Handled.ShouldBeTrue("Second request for value wasn't handled.");
                this.secondBuildResult.ShouldSetValue.ShouldBeTrue("Second request didn't indicate that the value should be set.");
            }

            [Test]
            public virtual void Should_generate_first_value_at_the_minimum_using_a_default_scale_of_3_digits()
            {
                this.firstBuildResult.Value.ShouldEqual(-1M);
            }

            [Test]
            public virtual void Should_generate_second_value_just_above_the_first_using_a_default_scale_of_3_digits()
            {
                this.secondBuildResult.Value.ShouldEqual(-.999M);
            }
        }
        
        [TestFixture]
        public class When_generating_two_values_for_a_decimal_property_where_the_minimum_is_the_default_value //: TestFixtureBase
        {
            private ValueBuildResult firstBuildResult;
            private ValueBuildResult secondBuildResult;
            
            [TestFixtureSetUp]
            public void Act()
            {
                var builder = new RangeConstrainedFloatingPointValueBuilder();
                var factory = new TestObjectFactory(new[] { builder }, null, new CustomAttributeProvider());

                this.firstBuildResult = GetValueBuildResultForProperty(builder, x => x.ZeroToTwentyWithNoScaleSupplied);
                this.secondBuildResult = GetValueBuildResultForProperty(builder, x => x.ZeroToTwentyWithNoScaleSupplied);
            }

            [Test]
            public virtual void Should_handle_both_requests_indicating_the_value_should_be_set()
            {
                this.firstBuildResult.Handled.ShouldBeTrue("First request for value wasn't handled.");
                this.firstBuildResult.ShouldSetValue.ShouldBeTrue("First request didn't indicate that the value should be set.");

                this.secondBuildResult.Handled.ShouldBeTrue("Second request for value wasn't handled.");
                this.secondBuildResult.ShouldSetValue.ShouldBeTrue("Second request didn't indicate that the value should be set.");
            }

            [Test]
            public virtual void Should_generate_first_value_just_above_the_default_value_using_the_default_scale_of_3_digits()
            {
                this.firstBuildResult.Value.ShouldEqual(.001M);
            }

            [Test]
            public virtual void Should_generate_second_value_just_above_the_first_using_a_default_scale_of_3_digits()
            {
                this.secondBuildResult.Value.ShouldEqual(.002M);
            }
        }

        [TestFixture]
        public class When_generating_two_values_for_a_decimal_property_with_precision_and_scale_that_can_be_inferred_from_the_range_values //: TestFixtureBase
        {
            private ValueBuildResult firstBuildResult;
            private ValueBuildResult secondBuildResult;

            [TestFixtureSetUp]
            public void Act()
            {
                var builder = new RangeConstrainedFloatingPointValueBuilder();
                var factory = new TestObjectFactory(new[] { builder }, null, new CustomAttributeProvider());

                this.firstBuildResult = GetValueBuildResultForProperty(builder, x => x.PrecisionEightScaleFive);
                this.secondBuildResult = GetValueBuildResultForProperty(builder, x => x.PrecisionEightScaleFive);
            }

            [Test]
            public virtual void Should_handle_both_requests_indicating_the_value_should_be_set()
            {
                this.firstBuildResult.Handled.ShouldBeTrue("First request for value wasn't handled.");
                this.firstBuildResult.ShouldSetValue.ShouldBeTrue("First request didn't indicate that the value should be set.");

                this.secondBuildResult.Handled.ShouldBeTrue("Second request for value wasn't handled.");
                this.secondBuildResult.ShouldSetValue.ShouldBeTrue("Second request didn't indicate that the value should be set.");
            }

            [Test]
            public virtual void Should_generate_first_value_at_the_range_minimum_using_the_scale_inferred_from_the_range_values()
            {
                this.firstBuildResult.Value.ShouldEqual(-999.99999M);
            }

            [Test]
            public virtual void Should_generate_second_value_just_above_the_first_using_the_scale_inferred_from_the_range_values()
            {
                this.secondBuildResult.Value.ShouldEqual(-999.99998M);
            }
        }

        [TestFixture]
        public class When_generating_values_for_a_Range_constrained_decimal_property_that_will_include_the_default_value_and_exceed_the_defined_maximum //: TestFixtureBase
        {
            private ValueBuildResult firstBuildResult;
            private ValueBuildResult secondBuildResult;
            private ValueBuildResult thirdBuildResult;

            [TestFixtureSetUp]
            public void Act()
            {
                var builder = new RangeConstrainedFloatingPointValueBuilder();
                var factory = new TestObjectFactory(new[] { builder }, null, new CustomAttributeProvider());

                this.firstBuildResult = GetValueBuildResultForProperty(builder, x => x.PrecisionTwoScaleTwo);
                this.secondBuildResult = GetValueBuildResultForProperty(builder, x => x.PrecisionTwoScaleTwo);
                this.thirdBuildResult = GetValueBuildResultForProperty(builder, x => x.PrecisionTwoScaleTwo);
            }

            [Test]
            public virtual void Should_handle_both_requests_indicating_the_value_should_be_set()
            {
                this.firstBuildResult.Handled.ShouldBeTrue("First request for value wasn't handled.");
                this.firstBuildResult.ShouldSetValue.ShouldBeTrue("First request didn't indicate that the value should be set.");

                this.secondBuildResult.Handled.ShouldBeTrue("Second request for value wasn't handled.");
                this.secondBuildResult.ShouldSetValue.ShouldBeTrue("Second request didn't indicate that the value should be set.");

                this.thirdBuildResult.Handled.ShouldBeTrue("Third request for value wasn't handled.");
                this.thirdBuildResult.ShouldSetValue.ShouldBeTrue("Third request didn't indicate that the value should be set.");
            }

            [Test]
            public virtual void Should_generate_first_value_at_the_Range_minimum()
            {
                this.firstBuildResult.Value.ShouldEqual(-.01M);
            }

            [Test]
            public virtual void Should_generate_second_value_skipping_the_default_value()
            {
                this.secondBuildResult.Value.ShouldEqual(.01M);
            }

            [Test]
            public virtual void Should_generate_third_value_rolling_back_to_the_minimum()
            {
                this.thirdBuildResult.Value.ShouldEqual(-.01M);
            }
        }
    }
}