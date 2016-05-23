// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Utilities.LoadGeneration.ValueBuilders;
using EdFi.TestObjects;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ValueBuilders
{
    public class StudentInstanceStudentUniqueIdValueBuilderFixtures
    {
        public class Student
        {
            /* A unique alpha-numeric code assigned to a student. */
            public string studentUniqueId { get; set; }

            /* A name given to an individual at birth, baptism, or during another naming ceremony, or through legal change.  NEDM: First Name */
            public string firstName { get; set; }

            /* The name borne in common by members of a family.  NEDM: Last Name/Surname */
            public string lastSurname { get; set; }

            /* A person''s gender. */
            public string sexType { get; set; }

            /* The month, day, and year on which an individual was born. */
            public DateTime? birthDate { get; set; }
        }

        public class When_considering_building_a_UniqueId_for_a_Student_instance : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private List<ValueBuildResult> _actualIncompleteBuildResults;
            private ValueBuildResult _actualCompleteBuildResult;

            // External dependencies
            private IUniqueIdFactory _uniqueIdFactory;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                _uniqueIdFactory = Stub<IUniqueIdFactory>();
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var builder = new PersonInstanceUniqueIdValueBuilder(_uniqueIdFactory);

                _actualIncompleteBuildResults = new List<ValueBuildResult>();

                // Generate contexts for incomplete identity information
                foreach (var buildContext in GetIncompleteBuildContexts())
                    _actualIncompleteBuildResults.Add(builder.TryBuild(buildContext));

                // Generate contexts for complete identity information
                var identityInfoCompleteStudent = new Student
                {
                    firstName = "Bob",
                    lastSurname = "Jones",
                    birthDate = DateTime.Today,
                    sexType = "Male",
                };

                var completeBuilderContext = new BuildContext("xxx.StudentUniqueId", typeof(string),
                        new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase),
                        typeof(Student), new LinkedList<object>(new [] {identityInfoCompleteStudent}), 
                        BuildMode.Create);

                _actualCompleteBuildResult = builder.TryBuild(completeBuilderContext);
            }

            private static IEnumerable<BuildContext> GetIncompleteBuildContexts()
            {
                // Build all combinations of incomplete identity values, for which UniqueId creation should be deferred
                for (int i = 1; i < 15; i++)
                {
                    var student = new Student();

                    if ((i & 1) != 0)
                        student.firstName = "Bob";

                    if ((i & 2) != 0)
                        student.lastSurname = "Jones";

                    if ((i & 4) != 0)
                        student.birthDate = DateTime.Today;

                    if ((i & 8) != 0)
                        student.sexType = "Male";

                    yield return new BuildContext("xxx.StudentUniqueId", typeof(string),
                        new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase),
                        typeof(Student), new LinkedList<object>(new [] {student}), 
                        BuildMode.Create);
                }
            }

            [Assert]
            public void Should_defer_building_the_value_when_generation_of_the_identity_information_is_still_not_complete()
            {
                _actualIncompleteBuildResults.All(x => x.ShouldDefer).ShouldBeTrue();
            }

            [Assert]
            public void Should_not_defer_building_the_value_once_all_identity_information_has_been_generated()
            {
                _actualCompleteBuildResult.ShouldDefer.ShouldBeFalse();
            }
        }

        public class When_building_a_UniqueId_for_a_Student_instance : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private ValueBuildResult _actualBuildResult;

            // External dependencies
            private IUniqueIdFactory _uniqueIdFactory;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                _uniqueIdFactory = Stub<IUniqueIdFactory>();
                _uniqueIdFactory.Expect(x => x.CreateUniqueId(null, null, default(DateTime), null))
                    .IgnoreArguments()
                    .Return("ABC");
            }

            protected override void Act()
            {
                var student = new Student
                {
                    firstName = "Bob",
                    lastSurname = "Jones",
                    birthDate = DateTime.Today,
                    sexType = "Male",
                };

                var completeBuilderContext = new BuildContext("xxx.StudentUniqueId", typeof(string),
                    new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase),
                    typeof(Student), new LinkedList<object>(new[] { student }),
                    BuildMode.Create);

                var builder = new PersonInstanceUniqueIdValueBuilder(_uniqueIdFactory);
                _actualBuildResult = builder.TryBuild(completeBuilderContext);
            }

            [Assert]
            public void Should_pass_Student_identity_information_to_unique_id_factory()
            {
                _uniqueIdFactory.AssertWasCalled(x => 
                    x.CreateUniqueId(
                        Arg<string>.Is.Equal("Bob"),
                        Arg<string>.Is.Equal("Jones"),
                        Arg<DateTime>.Is.Equal(DateTime.Today),
                        Arg<string>.Is.Equal("Male")
                    ));
            }

            [Assert]
            public void Should_return_build_value_containing_the_created_UniqueId()
            {
                _actualBuildResult.Value.ShouldEqual("ABC");
            }
        }
    }
}