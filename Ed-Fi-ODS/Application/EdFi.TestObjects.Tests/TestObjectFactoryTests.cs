using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.TestObjects.Builders;
using NUnit.Framework;
using Should;

namespace EdFi.TestObjects.Tests
{
    public class TestObjectFactoryTests
    {
        public class TargetClass
        {
            public string PropertyOne { get; set; }
            public int PropertyTwo { get; set; }
            public DateTime PropertyWithConstraint { get; set; }

            public TargetReferenceClass Reference { get; set; }

            public List<TargetChildClass> Children { get; set; }
        }

        public class TargetReferenceClass
        {
            public string PropertyThree { get; set; }
        }

        public class TargetChildClass
        {
            public long PropertyFour { get; set; }
        }

        public class TodayDateTimeValueBuilder : IValueBuilder
        {
            public ValueBuildResult TryBuild(BuildContext buildContext)
            {
                if (buildContext.TargetType == typeof(DateTime))
                    return ValueBuildResult.WithValue(DateTime.Today, buildContext.LogicalPropertyPath);

                return ValueBuildResult.NotHandled;
            }

            public void Reset() { }
            public ITestObjectFactory Factory { get; set; }
        }

        public class RecordingValueBuilder : IValueBuilder
        {
            public RecordingValueBuilder()
            {
                Calls = new List<BuildContext>();
            }

            public List<BuildContext> Calls { get; set; }

            public ValueBuildResult TryBuild(BuildContext buildContext)
            {
                var copyOfContext = new BuildContext(
                    buildContext.LogicalPropertyPath,
                    buildContext.TargetType,
                    new Dictionary<string, object>(buildContext.PropertyValueConstraints, StringComparer.InvariantCultureIgnoreCase),
                    buildContext.ContainingType,
                    new LinkedList<object>(buildContext.ObjectGraphLineage),
                    buildContext.BuildMode);

                Calls.Add(copyOfContext);

                return ValueBuildResult.NotHandled;
            }

            public void Reset() { }
            public ITestObjectFactory Factory { get; set; }
        }

        [TestFixture]
        public class When_creating_an_object_with_children
        {
            private TargetClass _actualCreated;
            private RecordingValueBuilder _recordingValueBuilder;

            [TestFixtureSetUp]
            public void Setup()
            {
                _recordingValueBuilder = new RecordingValueBuilder();

                var factory = new TestObjectFactory(
                    new IValueBuilder[]
                                {
                                    _recordingValueBuilder,
                                    new TodayDateTimeValueBuilder(), 
                                    new IEnumerableBuilder(), 
                                    new StringValueBuilder(), 
                                    new IncrementingNumericValueBuilder(), 
                                },
                    new SystemActivator(),
                    new CustomAttributeProvider());

                // Act
                var actualCreatedAsObject = factory.Create("", typeof(TargetClass), null, null);
                _actualCreated = (TargetClass)actualCreatedAsObject;
            }

            [Test]
            public void Should_attempt_to_create_child_with_the_object_graph_referencing_both_the_child_and_the_parent_objects()
            {
                var childObjectCalls = _recordingValueBuilder.Calls
                    .Where(x => x.TargetType == typeof(long)).ToList(); // Only the child class has a "long" property
                
                childObjectCalls.Count.ShouldBeGreaterThan(0);

                var actualChildObjectInstance = childObjectCalls.First().ObjectGraphLineage.First.Value;
                actualChildObjectInstance.ShouldBeType<TargetChildClass>();

                var actualParentObjectInstance = childObjectCalls.First().ObjectGraphLineage.First.Next.Value;
                actualParentObjectInstance.ShouldBeType<TargetClass>();
            }
        }

        [TestFixture]
        public class When_creating_an_object_where_some_properties_are_deferred
        {
            #region Test Value Builder
            public class OnceDeferredIntBuilder : IValueBuilder
            {
                private int deferCount;
                private int calledCount;
                private bool allOtherPropertiesHaveTheirValuesOnTheSecondCall;

                public ValueBuildResult TryBuild(BuildContext buildContext)
                {
                    if (buildContext.TargetType != typeof(int))
                        return ValueBuildResult.NotHandled;

                    calledCount = CalledCount + 1;

                    if (deferCount > 0)
                    {
                        var instance = buildContext.GetContainingInstance() as TargetClass;

                        allOtherPropertiesHaveTheirValuesOnTheSecondCall =
                            instance.PropertyOne != null
                            && instance.PropertyWithConstraint != default(DateTime)
                            && instance.Reference != null;

                        return ValueBuildResult.WithValue(99, buildContext.LogicalPropertyPath);
                    }

                    deferCount++;

                    return ValueBuildResult.Deferred;
                }

                public void Reset() { }

                public ITestObjectFactory Factory { get; set; }

                public int CalledCount
                {
                    get { return calledCount; }
                }

                public bool All_other_properties_have_their_values_on_the_second_call
                {
                    get { return allOtherPropertiesHaveTheirValuesOnTheSecondCall; }
                }
            }
            #endregion

            private TargetClass _actualCreated;
            private OnceDeferredIntBuilder suppliedOnceDeferredIntBuilder;

            [TestFixtureSetUp]
            public void Arrange()
            {
                var factory = new TestObjectFactory(
                    new IValueBuilder[]
                    {
                        new TodayDateTimeValueBuilder(),
                        new IEnumerableBuilder(),
                        new StringValueBuilder(), 
                        suppliedOnceDeferredIntBuilder = new OnceDeferredIntBuilder(), 
                        new IncrementingNumericValueBuilder(), 
                    },
                    new SystemActivator(),
                    new CustomAttributeProvider());

                Act(factory);
            }

            private void Act(TestObjectFactory factory)
            {
                var actualCreatedAsObject = factory.Create("", typeof(TargetClass),
                    new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase), null);

                _actualCreated = (TargetClass)actualCreatedAsObject;
            }

            [Test]
            public void Should_handle_the_defer_response_the_first_time_through_and_call_again_to_retrieve_the_property()
            {
                suppliedOnceDeferredIntBuilder.CalledCount.ShouldEqual(2);
            }

            [Test]
            public void Should_have_built_all_other_properties_before_calling_again_to_build_the_deferred_property()
            {
                suppliedOnceDeferredIntBuilder
                    .All_other_properties_have_their_values_on_the_second_call
                    .ShouldBeTrue();
            }

            [Test]
            public void Should_build_the_integer_value_on_the_second_attempt()
            {
                _actualCreated.PropertyTwo.ShouldEqual(99);
            }
        }

        [TestFixture]
        public class When_creating_an_object_where_some_properties_are_deferred_multiple_times_but_resolve
        {
            #region Test value builders

            /// <summary>
            /// Builds integers, deferring on the first request, and returning a '99' the second time.
            /// </summary>
            public class OnceDeferredIntBuilder : IValueBuilder
            {
                private int deferCount;
                private int calledCount;
                private bool otherPropertiesHaveValuesOnSecondCall;

                public ValueBuildResult TryBuild(BuildContext buildContext)
                {
                    if (buildContext.TargetType != typeof(int))
                        return ValueBuildResult.NotHandled;

                    calledCount = CalledCount + 1;

                    if (deferCount > 0)
                    {
                        var instance = buildContext.GetContainingInstance() as TargetClass;

                        otherPropertiesHaveValuesOnSecondCall =
                            instance.PropertyOne != null
                            && instance.PropertyWithConstraint != default(DateTime)
                            && instance.Reference != null;

                        return ValueBuildResult.WithValue(99, buildContext.LogicalPropertyPath);
                    }

                    deferCount++;

                    return ValueBuildResult.Deferred;
                }

                public void Reset()
                {
                }

                public ITestObjectFactory Factory { get; set; }

                public int CalledCount
                {
                    get { return calledCount; }
                }

                public bool OtherPropertiesHaveValuesOnSecondCall
                {
                    get { return otherPropertiesHaveValuesOnSecondCall; }
                }
            }

            #endregion

            public class DependentOnIntPropertyStringBuilder : IValueBuilder
            {
                private int calledCount;

                public ValueBuildResult TryBuild(BuildContext buildContext)
                {
                    var instance = buildContext.GetContainingInstance() as TargetClass;

                    if (buildContext.TargetType != typeof(string)
                        || instance == null)
                        return ValueBuildResult.NotHandled;

                    calledCount = CalledCount + 1;

                    if (instance.PropertyTwo == default(int))
                        return ValueBuildResult.Deferred;

                    return ValueBuildResult.WithValue(
                        string.Format("Deferred String (int={0})", instance.PropertyTwo), 
                        buildContext.LogicalPropertyPath);
                }

                public void Reset() { }

                public ITestObjectFactory Factory { get; set; }

                public int CalledCount
                {
                    get { return calledCount; }
                }
            }

            private TargetClass _actualCreated;
            private OnceDeferredIntBuilder suppliedOnceDeferredIntBuilder;
            private DependentOnIntPropertyStringBuilder suppliedDependentOnIntPropertyStringBuilder;

            [TestFixtureSetUp]
            public void Arrange()
            {
                var factory = new TestObjectFactory(
                    new IValueBuilder[]
                    {
                        new TodayDateTimeValueBuilder(),
                        new IEnumerableBuilder(),
                        suppliedDependentOnIntPropertyStringBuilder = new DependentOnIntPropertyStringBuilder(), 
                        suppliedOnceDeferredIntBuilder = new OnceDeferredIntBuilder(), 
                        new IncrementingNumericValueBuilder(), 
                        new StringValueBuilder(), 
                    },
                    new SystemActivator(),
                    new CustomAttributeProvider());

                Act(factory);
            }

            private void Act(TestObjectFactory factory)
            {
                var actualCreatedAsObject = factory.Create("", typeof(TargetClass),
                    new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase), null);

                _actualCreated = (TargetClass)actualCreatedAsObject;
            }

            [Test]
            public void Should_defer_building_the_string_until_the_int_builder_completes()
            {
                int intBuildingCalls = suppliedOnceDeferredIntBuilder.CalledCount;

                suppliedDependentOnIntPropertyStringBuilder.CalledCount.ShouldEqual(intBuildingCalls + 1);
            }

            [Test]
            public void Should_incorporate_the_integer_value_into_the_string()
            {
                _actualCreated.PropertyOne.ShouldEqual("Deferred String (int=99)");
            }
        }

        [TestFixture]
        public class When_creating_an_object_where_one_of_the_properties_keeps_deferring
        {
            public class AlwaysDefersIntBuilder : IValueBuilder
            {
                private int deferCount;

                public ValueBuildResult TryBuild(BuildContext buildContext)
                {
                    if (buildContext.TargetType != typeof(int))
                        return ValueBuildResult.NotHandled;

                    deferCount++;

                    if (deferCount >= 5)
                        throw new Exception("Repeating defer response was not caught correctly.");

                    return ValueBuildResult.Deferred;
                }

                public void Reset() { }

                public ITestObjectFactory Factory { get; set; }
            }

            private Exception _actualException;

            [TestFixtureSetUp]
            public void Arrange()
            {
                var factory = new TestObjectFactory(
                    new IValueBuilder[]
                    {
                        new TodayDateTimeValueBuilder(),
                        new IEnumerableBuilder(),
                        new AlwaysDefersIntBuilder(), 
                        new IncrementingNumericValueBuilder(), 
                        new StringValueBuilder(), 
                    },
                    new SystemActivator(),
                    new CustomAttributeProvider());

                Act(factory);
            }

            private void Act(TestObjectFactory factory)
            {
                try
                {
                    factory.Create("", typeof(TargetClass), new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase), null);
                }
                catch (Exception ex)
                {
                    _actualException = ex;
                }
            }

            [Test]
            public void Should_detect_the_recurring_defer_response_and_throw_an_exception()
            {
                _actualException.ShouldNotBeNull();
                _actualException.InnerException.Message.ShouldContain("Defer");
            }
        }


        [TestFixture]
        public class When_creating_an_object_with_property_value_constraints
        {
            private TargetClass _actualCreated;

            [TestFixtureSetUp]
            public void Arrange()
            {
                var factory = new TestObjectFactory(
                    new IValueBuilder[]
                    {
                        new TodayDateTimeValueBuilder(),
                        new IEnumerableBuilder(),
                        new StringValueBuilder(), 
                        new IncrementingNumericValueBuilder(), 
                    },
                    new SystemActivator(),
                    new CustomAttributeProvider());

                Act(factory);
            }

            private void Act(TestObjectFactory factory)
            {
                var actualCreatedAsObject = factory.Create("", typeof(TargetClass),
                    new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
                    {
                        {"PropertyOne", "Hello"},
                        {"PropertyTwo", 22},
                        {"PropertyThree", "World"},
                        {"PropertyFour", 44},
                    }, null);

                _actualCreated = (TargetClass) actualCreatedAsObject;
            }

            [Test]
            public void Should_build_property_values_on_main_target_using_supplied_property_value_constraints()
            {
                // Make sure all provided property value constraints are applied, including on referenced objects
                _actualCreated.PropertyOne.ShouldEqual("Hello");
                _actualCreated.PropertyTwo.ShouldEqual(22);
            }

            [Test]
            public void Should_build_property_values_on_referenced_objects_using_supplied_property_value_constraints()
            {
                // Constrain values into object references
                _actualCreated.Reference.PropertyThree.ShouldEqual("World");
            }

            [Test]
            public void Should_NOT_build_property_values_on_child_objects_in_lists_using_supplied_property_value_constraints()
            {
                // Constraint values SHOULD NOT flow into children (lists)
                _actualCreated.Children.All(c => c.PropertyFour == 44).ShouldBeFalse();
            }

            [Test]
            public void Should_use_the_value_builders_for_values_not_defined_in_constraints()
            {
                // Use value builder for values not provided in the constraints
                _actualCreated.PropertyWithConstraint.ShouldEqual(DateTime.Today);
            }
        }

        [TestFixture]
        public class When_creating_an_object_with_property_value_constraints_using_arbitrary_casing
        {
            private TargetClass _actualCreated;

            [TestFixtureSetUp]
            public void Arrange()
            {
                var factory = new TestObjectFactory(
                    new IValueBuilder[]
                    {
                        new TodayDateTimeValueBuilder(),
                        new IEnumerableBuilder(),
                        new StringValueBuilder(),
                        new IncrementingNumericValueBuilder(),
                    },
                    new SystemActivator(),
                    new CustomAttributeProvider());

                Act(factory);
            }

            private void Act(TestObjectFactory factory)
            {
                var actualCreatedAsObject = factory.Create("", typeof(TargetClass),
                    new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
                    {
                        {"prOpertyOne", "Hello"},
                        {"prOpertyTwo", 22},
                    }, null);

                _actualCreated = (TargetClass) actualCreatedAsObject;
            }

            [Test]
            public void Should_build_property_values_on_main_target_using_supplied_property_value_constraints_even_if_casing_does_not_match()
            {
                // Make sure all provided property value constraints are applied, including on referenced objects
                _actualCreated.PropertyOne.ShouldEqual("Hello");
                _actualCreated.PropertyTwo.ShouldEqual(22);
            }
        }

        [TestFixture]
        public class When_creating_an_object_with_property_value_constraints_using_the_role_name_prefix_convention
        {
            private TargetClass _actualCreated;

            [TestFixtureSetUp]
            public void Arrange()
            {
                var factory = new TestObjectFactory(
                    new IValueBuilder[]
                    {
                        new TodayDateTimeValueBuilder(),
                        new IEnumerableBuilder(),
                        new StringValueBuilder(), 
                        new IncrementingNumericValueBuilder(), 
                    },
                    new SystemActivator(),
                    new CustomAttributeProvider());

                Act(factory);
            }

            private void Act(TestObjectFactory factory)
            {
                var actualCreatedAsObject = factory.Create("", typeof(TargetClass),
                    new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
                    {
                        {"TargetClassPropertyOne", "Hello"},
                        {"TargetClassPropertyTwo", 22},
                    }, null);

                _actualCreated = (TargetClass)actualCreatedAsObject;
            }

            [Test]
            public void Should_build_property_values_on_main_target_using_supplied_property_value_constraints()
            {
                // Make sure all provided property value constraints are applied, including on referenced objects
                _actualCreated.PropertyOne.ShouldEqual("Hello");
                _actualCreated.PropertyTwo.ShouldEqual(22);
            }
        }
    }
}
