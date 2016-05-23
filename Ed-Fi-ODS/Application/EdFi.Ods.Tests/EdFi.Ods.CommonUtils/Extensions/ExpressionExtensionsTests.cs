namespace EdFi.Ods.Tests.EdFi.Ods.CommonUtils.Extensions
{
    using System;
    using System.Linq.Expressions;

    using global::EdFi.Ods.Common.Utils.Extensions;

    using NUnit.Framework;

    using Should;

    public class ExpressionExtensionsTests
    {
        public class ExampleClass
        {
            public string SomeProperty { get; set; }
            public string SomeMethod()
            {
                return string.Empty;
            }
            public Guid SomeUnary { get; set; }

        }

        [TestFixture]
        public class When_getting_the_member_name_of_a_property
        {
            [Test]
            public void Should_get_the_member_name()
            {
                Expression<Func<ExampleClass, object>> expression = x => x.SomeProperty;

                expression.MemberName().ShouldEqual("SomeProperty");
            }
        }

        [TestFixture]
        public class When_getting_the_member_name_of_a_method
        {
            [Test]
            public void Should_get_the_member_name()
            {
                Expression<Func<ExampleClass, object>> expression = x => x.SomeMethod();

                expression.MemberName().ShouldEqual("SomeMethod");
            }    
        }

        [TestFixture]
        public class When_getting_the_member_name_of_a_unary_expression
        {
            [Test]
            public void Should_get_the_member_name()
            {
                Expression<Func<ExampleClass, object>> expression = x => x.SomeUnary;

                expression.MemberName().ShouldEqual("SomeUnary");
            }
        }
    }
}