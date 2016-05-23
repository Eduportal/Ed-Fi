// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.TestObjects;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public class ContextSpecificReferenceValueBuilderFixtures
    {
        public class SomeReference { }
        public class NotAReferenceType { }

        public class When_evaluating_whether_to_handle_building_a_value_for_a_type_whose_name_DOES_NOT_end_with_Reference : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private ValueBuildResult actualBuildResult;
            private Exception actualException;

            // External dependencies
            private IContextSpecificReferenceValueBuilder[] contextSpecificReferenceValueBuilders;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                var alwaysThrowExceptionBuilder = Stub<IContextSpecificReferenceValueBuilder>();

                alwaysThrowExceptionBuilder
                    .Expect(x => x.TryBuild(new BuildContext(null, typeof(object), null, null, null, BuildMode.Create)))
                    .IgnoreArguments()
                    .Throw(new InvalidOperationException("Should not have called the chain."));

                contextSpecificReferenceValueBuilders = new[] { alwaysThrowExceptionBuilder };
            }

            protected override void Act()
            {
                try
                {
                    // Perform the action to be tested
                    var builder = new ContextSpecificReferenceValueBuilder(contextSpecificReferenceValueBuilders);
                    actualBuildResult = builder.TryBuild(new BuildContext("", typeof(NotAReferenceType), new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase), null, null, BuildMode.Create));
                }
                catch (Exception ex)
                {
                    actualException = ex;
                }
            }

            [Assert]
            public void Should_not_handle_the_request()
            {
                actualBuildResult.Handled.ShouldBeFalse();
            }

            [Assert]
            public void Should_not_call_the_inner_chain()
            {
                // Mocked inner chain throws InvalidOperationException if called
                actualException.ShouldBeNull();
            }
        }

        public class When_evaluating_whether_to_handle_building_a_value_for_a_type_whose_name_ends_with_Reference_and_the_reference_is_handled_by_the_chain : TestFixtureBase
        {
            // Supplied values
            private ValueBuildResult suppliedValueBuildResultForInnerChain;

            // Actual values
            private ValueBuildResult actualBuildResult;

            // External dependencies
            private IContextSpecificReferenceValueBuilder[] contextSpecificReferenceValueBuilders;

            protected override void Arrange()
            {
                suppliedValueBuildResultForInnerChain = ValueBuildResult.WithValue(null, string.Empty);

                // Set up mocked dependences and supplied values
                var alwaysTrueBuilder = Stub<IContextSpecificReferenceValueBuilder>();

                alwaysTrueBuilder
                    .Expect(x => x.TryBuild(new BuildContext(null, typeof(object), null, null, null, BuildMode.Create)))
                    .IgnoreArguments()
                    .Return(suppliedValueBuildResultForInnerChain);

                contextSpecificReferenceValueBuilders = new[] { alwaysTrueBuilder };
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new ContextSpecificReferenceValueBuilder(contextSpecificReferenceValueBuilders);
                actualBuildResult = builder.TryBuild(new BuildContext("", typeof(SomeReference), new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase), null, null, BuildMode.Create));
            }

            [Assert]
            public void Should_call_the_inner_chain_of_IContextSpecificReferenceValueBuilders_to_obtain_the_build_result()
            {
                actualBuildResult.ShouldBeSameAs(suppliedValueBuildResultForInnerChain);
            }
        }



        public class When_evaluating_whether_to_handle_building_a_value_for_a_type_whose_name_ends_with_Reference_and_the_reference_IS_NOT_handled_by_the_chain : TestFixtureBase
        {
            public class NotHandledValueBuilder : IContextSpecificReferenceValueBuilder
            {
                public static int CalledCount { get; set; }

                public ValueBuildResult TryBuild(BuildContext buildContext)
                {
                    CalledCount++;
                    return ValueBuildResult.NotHandled;
                }

                public void Reset() {}
                public ITestObjectFactory Factory { get; set; }

                internal static void ResetStaticCounter()
                {
                    // Reset the counter
                    CalledCount = 0;
                }
            }            
            
            // Supplied values
            private ValueBuildResult suppliedValueBuildResultForInnerChain;

            // Actual values
            private ValueBuildResult actualBuildResult;

            // External dependencies
            private IContextSpecificReferenceValueBuilder[] contextSpecificReferenceValueBuilders;

            protected override void Arrange()
            {
                // Create an array of handlers that don't handle anything
                contextSpecificReferenceValueBuilders = new[] { new NotHandledValueBuilder(), new NotHandledValueBuilder() };
            }

            protected override void Act()
            {
                // Perform the action to be tested
                NotHandledValueBuilder.ResetStaticCounter();
                var builder = new ContextSpecificReferenceValueBuilder(contextSpecificReferenceValueBuilders);
                actualBuildResult = builder.TryBuild(new BuildContext("", typeof(SomeReference), new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase), null, null, BuildMode.Create));
            }

            [Assert]
            public void Should_call_each_member_of_the_chain()
            {
                NotHandledValueBuilder.CalledCount.ShouldEqual(contextSpecificReferenceValueBuilders.Length);
            }

            [Assert]
            public void Should_indicate_the_value_was_not_handled()
            {
                actualBuildResult.Handled.ShouldBeFalse();
            }
        }

        public class When_attempting_to_reset_the_ContextSpecificReferenceValueBuilder : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private Exception actualException;

            // External dependencies

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                
            }

            protected override void Act()
            {
                try
                {
                    var builder = new ContextSpecificReferenceValueBuilder(null);
                    builder.Reset();
                }
                catch (Exception ex)
                {
                    actualException = ex;                    
                }
            }

            [Assert]
            public void Should_not_throw_an_exception()
            {
                actualException.ShouldBeNull();
            }
        }
    }
}