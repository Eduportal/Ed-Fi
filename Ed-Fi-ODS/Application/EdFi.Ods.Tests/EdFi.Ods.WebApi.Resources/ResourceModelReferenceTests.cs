// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

namespace EdFi.Ods.Tests.EdFi.Ods.WebApi.Resources
{
    using System;

    using global::EdFi.Ods.Api.Models.Resources.StaffEducationOrganizationAssignmentAssociation;
    using global::EdFi.Ods.Api.Models.Resources.StudentSchoolAttendanceEvent;
    using global::EdFi.Ods.Api.Models.Resources.BellSchedule;
    using global::EdFi.Ods.Entities.Common;
    using global::EdFi.Ods.Tests._Bases;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using Should;

    public class ResourceModelReferenceTests
    {
        public class When_initalizing_a_model_with_an_optional_reference_where_none_of_the_optional_references_properties_are_explicitly_set : TestFixtureBase
        {
            // Supplied values
            private StaffEducationOrganizationAssignmentAssociation model;
            private IStaffEducationOrganizationAssignmentAssociation modelInterface;

            // Actual values
            private string actualSerializedJson;

            // External dependencies

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
            }

            protected override void Act()
            {
                // Perform the action to be tested
                this.model = new StaffEducationOrganizationAssignmentAssociation();
                
                // Populate the instance's value, including the shared FK value, but none of the optional reference's values
                this.modelInterface = this.model;

                // Set only the required "key" properties on the main model
                this.modelInterface.StaffUniqueId = "ABC123";  // This also serves as part of the optional reference, but is "unified" (or shared) from other references.
                this.modelInterface.EducationOrganizationId = 123;
                this.modelInterface.StaffClassificationDescriptor = "Hello";
                this.modelInterface.BeginDate = DateTime.Today;

                // NOTE: No other properties for the optional reference have been set

                this.actualSerializedJson = JsonConvert.SerializeObject(this.model);
            }

            [Assert]
            public void Should_not_return_default_values_for_nullable_properties_from_optional_reference()
            {
                // Assert the expected results
                this.modelInterface.EmploymentHireDate.ShouldBeNull();
                this.modelInterface.EmploymentEducationOrganizationId.ShouldBeNull();
                this.modelInterface.EmploymentStatusDescriptor.ShouldBeNull();
            }

            [Assert]
            public void Should_not_include_the_partially_defined_reference_in_JSON_output()
            {
                this.actualSerializedJson.ShouldContain(@"""employmentStaffEducationOrganizationEmploymentAssociationReference"":null");
            }

            [Assert]
            public void Should_not_introduce_a_partially_defined_reference_on_JSON_deserialization()
            {
                var deserializedModel = JsonConvert.DeserializeObject<StaffEducationOrganizationAssignmentAssociation>(this.actualSerializedJson);

                deserializedModel.EmploymentStaffEducationOrganizationEmploymentAssociationReference.ShouldBeNull();
            }
        }

        public class When_setting_and_getting_properties_for_an_optional_reference_on_a_model : TestFixtureBase
        {
            private StaffEducationOrganizationAssignmentAssociation model;
            private IStaffEducationOrganizationAssignmentAssociation modelInterface;

            [Assert]
            public void Should_not_return_values_from_optional_reference_until_all_of_the_references_properties_have_been_set()
            {
                this.model = new StaffEducationOrganizationAssignmentAssociation();
                this.modelInterface = this.model;

                // This property is "unified" with other references
                this.modelInterface.StaffUniqueId = "ABC123";

                // Because it's shared by multiple references, this property should return its value 
                // immediately (it's not **specific** to the optional reference)
                this.modelInterface.StaffUniqueId.ShouldEqual("ABC123");

                this.All_optional_reference_properties_should_still_return_null_values();

                // ---------------------------------------------------------
                // These properties are specific to the optional reference
                // ---------------------------------------------------------
                this.modelInterface.EmploymentEducationOrganizationId = 123;
                this.All_optional_reference_properties_should_still_return_null_values();
                this.Optional_reference_should_still_be_null();

                this.modelInterface.EmploymentStatusDescriptor = "XYZ";
                this.All_optional_reference_properties_should_still_return_null_values();
                this.Optional_reference_should_still_be_null();

                // After the last property specific to the optional reference is set, they should all return values (reference is complete)
                this.modelInterface.EmploymentHireDate = DateTime.Today;
                this.All_of_the_optional_reference_properties_should_now_return_values();
                this.Optional_reference_should_NOT_be_null();
            }

            private void Optional_reference_should_NOT_be_null()
            {
                this.model.EmploymentStaffEducationOrganizationEmploymentAssociationReference.ShouldNotBeNull();
            }

            private void Optional_reference_should_still_be_null()
            {
                this.model.EmploymentStaffEducationOrganizationEmploymentAssociationReference.ShouldBeNull();
            }

            private void All_optional_reference_properties_should_still_return_null_values()
            {
                this.modelInterface.EmploymentEducationOrganizationId.ShouldBeNull();
                this.modelInterface.EmploymentStatusDescriptor.ShouldBeNull();
                this.modelInterface.EmploymentHireDate.ShouldBeNull();
            }

            private void All_of_the_optional_reference_properties_should_now_return_values()
            {
                this.modelInterface.EmploymentEducationOrganizationId.ShouldNotEqual(null);
                this.modelInterface.EmploymentStatusDescriptor.ShouldNotEqual(null);
                this.modelInterface.EmploymentHireDate.ShouldNotEqual(null);
            }
        }

        [TestFixture]
        public class When_setting_and_getting_properties_for_a_required_reference_on_a_model
        {
            private StaffEducationOrganizationAssignmentAssociation model;
            private IStaffEducationOrganizationAssignmentAssociation modelInterface;

            [SetUp]
            protected void TestSetup()
            {
                this.model = new StaffEducationOrganizationAssignmentAssociation();
                this.modelInterface = this.model as IStaffEducationOrganizationAssignmentAssociation;
            }

            [Test]
            public void Reference_should_be_null_before_property_value_is_set()
            {
                this.model.StaffReference.ShouldBeNull();
            }

            [Test]
            public void Reference_should_NOT_be_null_after_property_value_is_set()
            {
                this.modelInterface.StaffUniqueId = "ABC123";
                this.model.StaffReference.ShouldNotBeNull();
            }

            [Test]
            public void Reference_should_be_null_again_after_property_value_is_returned_to_its_default_value()
            {
                this.modelInterface.StaffUniqueId = "ABC123";
                this.modelInterface.StaffUniqueId = default(string);
                this.model.StaffReference.ShouldBeNull();
            }

            [Test]
            public void Property_should_be_cleared_by_explicitly_setting_reference_to_null()
            {
                this.modelInterface.StaffUniqueId = "ABC123";
                this.model.StaffReference = null;
                this.modelInterface.StaffUniqueId.ShouldBeNull();
            }

            [Test]
            public void Reference_should_be_implicitly_restored_by_setting_the_property_value()
            {
                this.model.StaffReference = null;
                this.modelInterface.StaffUniqueId = "ABC123";
                this.model.StaffReference.ShouldNotBeNull();
                this.modelInterface.StaffUniqueId.ShouldEqual("ABC123");
            }
        }

        public class When_deserializing_a_model_with_a_reference_based_on_a_composite_key : TestFixtureBase
        {
            private StudentSchoolAttendanceEvent model;

            protected override void Act()
            {
                string json = @"
                    {
                        ""id"" : ""00000000000000000000000000000000"",
                        ""schoolReference"" : null,
                        ""sessionReference"" : {
                            ""schoolId"" : 123,
                            ""termDescriptor"" : ""ABC"",
                            ""schoolYear"" : 2013,
                            ""link"" : {
                                ""rel"" : ""Session"",
                                ""href"" : ""/sessions?schoolId=123&termType=ABC&schoolYear=2013""
                            }
                        },
                        ""studentReference"" : null,
                        ""eventDate"" : ""0001-01-01T00:00:00"",
                        ""attendanceEventCategoryDescriptor"" : null,
                        ""attendanceEventReason"" : null,
                        ""educationalEnvironmentType"" : null,
                        ""_etag"" : null
                    }
                ";

                this.model = JsonConvert.DeserializeObject<StudentSchoolAttendanceEvent>(json);
            }

            [Assert]
            public void Should_return_a_reference()
            {
                this.model.SessionReference.ShouldNotBeNull();
            }

            [Assert]
            public void Reference_should_return_the_non_default_values()
            {
                this.model.SessionReference.SchoolId.ShouldEqual(123);
                this.model.SessionReference.TermDescriptor.ShouldEqual("ABC");
                ((int)this.model.SessionReference.SchoolYear).ShouldEqual(2013);
            }
        }

        public class When_deserializing_a_model_with_a_reference_based_on_a_composite_key_with_backReference_presence : TestFixtureBase
        {
            private BellSchedule _model;
/* C# initializer helper
            private BellSchedule _model = new BellSchedule
                {
                    BellScheduleName = "ScheduleName",
                    BellScheduleMeetingTimes = new[]
                    {
                        new BellScheduleMeetingTime
                        {
                            ClassPeriodReference = new BellScheduleMeetingTimeToClassPeriodReference
                            {
                                ClassPeriodName = "ABC"
                            },
                            StartTime = DateTime.Now.TimeOfDay,
                            EndTime = new TimeSpan(1, 0, 0),
                        }
                    },
                    CalendarDateReference = new CalendarDateReference
                    {
                        Date = DateTime.Now,
                        EducationOrganizationId = 1,
                    },
                    SchoolReference = new SchoolReference
                    {
                        SchoolId = 10
                    },
                    GradeLevelDescriptor = "A",
                };
*/

            protected override void Act()
            {
                const string json = @"
{
    'calendarDateReference' : {
        'Date' : '2000-01-01',
        'schoolId' : 1,
    },
    'schoolReference' : {
        'schoolId' : 10
    },
    'gradeLevelDescriptor' : 'A',
    'name' : 'ScheduleName',
    'meetingTimes' : [
        {
            'classPeriodReference' : {
                'name' : 'ABC',
                'schoolId' : 10,
            },
            'startTime' : '00:00:00',
            'endTime' : '01:00:00',
        }
    ],
}
";
                _model = JsonConvert.DeserializeObject<BellSchedule>(json);
            }

            [Assert]
            public void Should_return_a_reference()
            {
                _model.BellScheduleMeetingTimes[0].ClassPeriodReference.ShouldNotBeNull();
            }

            [Assert]
            public void Reference_should_return_the_non_default_values()
            {
                _model.BellScheduleMeetingTimes[0].ClassPeriodReference.ClassPeriodName.ShouldEqual("ABC");
            }
        }
    }
}