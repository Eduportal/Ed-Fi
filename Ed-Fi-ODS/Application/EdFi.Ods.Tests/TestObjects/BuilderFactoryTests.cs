using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.TestObjects;
using EdFi.TestObjects.Builders;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.TestObjects
{
    using global::EdFi.Ods.Tests.TestObjects.CustomBuilders;

    public class BuilderFactoryTests
    {
        [TestFixture]
        public class When_getting_list_of_builders
        {
            private IEnumerable<Type> _builderTypes;

            [TestFixtureSetUp]
            public void Setup()
            {
                var factory = new BuilderFactory();
                _builderTypes = factory.GetBuilders().Select(x => x.GetType());
            }

            [Test]
            public void Should_include_all_declared_builders()
            {
                var declaredBuilders = typeof(IValueBuilder).Assembly.GetTypes()
                                                             .Where(x => typeof(IValueBuilder).IsAssignableFrom(x))
                                                             .Except(new []{typeof(NamedPropertySkipper<>)})
                                                             .Except(new []{typeof(AutoIncrementAttributedIdSkipper)})
                                                             .Except(new []{typeof(EducationOrganizationIdBuilder)})
                                                             .Except(new[]{typeof(AddedFirst)})
                                                             .Except(new[]{typeof(AddedSecond)})
                                                             .Except(new[]
                                                             {
                                                                 // TODO: GKM - This original intent of this test should be reviewed, and possibly the test deprecated.  The TestObjects library now has value builders that are optional depending on the context in which they're used.
                                                                 typeof(RandomNumericValueBuilder),
                                                                 typeof(RangeConstrainedRandomFloatingPointValueBuilder),
                                                             
                                                             })
                                                             .Where(x => !x.IsAbstract)
                                                             .ToArray();

                foreach (var declaredBuilder in declaredBuilders)
                {
                    _builderTypes.ShouldContain(declaredBuilder);
                }
            }
        }

        public abstract class AbstractBuilderForTest : IValueBuilder
        {
            public ValueBuildResult TryBuild(BuildContext buildContext)
            {
                throw new NotImplementedException();
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public ITestObjectFactory Factory
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }

        private class AddedFirst : AbstractBuilderForTest
        {
        }

        private class AddedSecond : AbstractBuilderForTest
        {
        }


        [TestFixture]
        public class When_two_builders_are_added_to_the_front
        {
            private IValueBuilder[] _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var factory = new BuilderFactory();

                factory.ClearAddOnBuilders();
                factory.AddToFront<AddedFirst>();
                factory.AddToFront<AddedSecond>();

                _result = factory.GetBuilders();
            }

            [Test]
            public void Should_provide_the_last_builder_that_was_added_to_the_front_as_the_first_builder()
            {
                _result[0].GetType().ShouldEqual(typeof(AddedSecond));
                _result[1].GetType().ShouldEqual(typeof(AddedFirst));
            }
        }

        [TestFixture]
        public class When_two_builders_are_added_to_the_end
        {
            private IValueBuilder[] _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var factory = new BuilderFactory();

                factory.ClearAddOnBuilders();
                factory.AddToEnd<AddedFirst>();
                factory.AddToEnd<AddedSecond>();

                _result = factory.GetBuilders();
            }

            [Test]
            public void Should_provide_the_last_builder_that_was_added_to_the_front_as_the_last_builder_before_the_custom_object_builder()
            {
                var length = _result.Length;

                _result[length - 2].GetType().ShouldEqual(typeof(AddedFirst));
                _result[length - 1].GetType().ShouldEqual(typeof(AddedSecond));
            }
        }
    }
}