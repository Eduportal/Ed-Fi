using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using EdFi.Ods.Tests._Bases;
using NUnit.Framework;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public class ResourceReferenceFactoryFixtures
    {
        #region Models for initial unit testing
        public class SomeClass
        {
            public AnotherClassReference AnotherClassReference { get; set; }
            public AnotherClassReference AnOptionalReference { get; set; }
            public StringBuilder StringBuilder { get; set; }
            public string ReferenceStringProperty { get; set; }
            public int ReferenceIntProperty { get; set; }
            public decimal UnneededDecimal { get; set; }
            public string  UnneededString { get; set; }
        }

        public class SomeClassReference
        {
            public string ReferenceStringProperty { get; set; }
            public int ReferenceIntProperty { get; set; }
            public DateTime ReferenceDateTimeProperty { get; set; }
            public int Capacity { get; set; } // This is here to make sure the .NET classes (e.g. StringBuilder) isn't processed when searching for values
        }

        public class AnotherClassReference
        {
            public DateTime ReferenceDateTimeProperty { get; set; }
            public long UnneededLong { get; set; }
            public int UnneededInt { get; set; }
        }
        #endregion

        #region Models for role name convention processing

        public class GradingPeriodReference
        {
            /* EducationOrganization Identity Column */
            public int? educationOrganizationId { get; set; }

            /* The name of the grading period during the school year in which the grade is offered (e.g., 1st cycle, 1st semester)   */
            public string descriptor { get; set; }

            /* Month, day, and year of the first day of the grading period.     */
            public DateTime? beginDate { get; set; }
        }
        
        public class GradingPeriod
        {
            /* The unique identifier of the resource. */
            public string id { get; set; }

            /* A reference to the related CalendarDate resource. */
            public CalendarDateReference beginCalendarDateReference { get; set; }

            /* A reference to the related CalendarDate resource. */
            public CalendarDateReference endCalendarDateReference { get; set; }

            /* The name of the grading period during the school year in which the grade is offered (e.g., 1st cycle, 1st semester)   */
            public string descriptor { get; set; }
        }

        public class CalendarDateReference
        {
            /* EducationOrganization Identity Column */
            public int? educationOrganizationId { get; set; }

            /* Month, day, and year of the first day of the grading period.     */
            public DateTime? date { get; set; }
        }

        #endregion

        #region Models for class name prefix trimmed convention testing

        public class SectionReference
        {
            /* School Identity Column */
            public int? schoolId { get; set; }

            /* An indication of the portion of a typical daily session in which students receive instruction in a specified subject (e.g., morning, sixth period, block period, or AB schedules).   NEDM: Class Period   */
            public string classPeriodName { get; set; }
        }

        public class Section
        {
            /* The unique identifier of the resource. */
            public string id { get; set; }

            /* A reference to the related ClassPeriod resource. */
            public ClassPeriodReference classPeriodReference { get; set; }
        }

        public class ClassPeriodReference
        {
            /* School Identity Column */
            public int? schoolId { get; set; }

            /* An indication of the portion of a typical daily session in which students receive instruction in a specified subject (e.g., morning, sixth period, block period, or AB schedules).   NEDM: Class Period   */
            public string name { get; set; }

        }

        #endregion

        #region Models for class has and identifier that has class name as prefix, and ends with "id"
        public class LearningStandardReference
        {
            /* The Identifier for the specific learning standard (e.g., 111.15.3.1.A) */
            public string learningStandardId { get; set; }

        }

        public class LearningStandard
        {
            /* The unique identifier of the resource. */
            public string id { get; set; }

            /* The Identifier for the specific learning standard (e.g., 111.15.3.1.A) */
            public string learningStandardId { get; set; }

        }
        #endregion

        public class When_creating_a_reference_from_an_instance_that_has_references_with_role_names : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private GradingPeriodReference _actualReference;

            // External dependencies
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                Type ignored;
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(x => x.TryGetReferenceType(typeof(GradingPeriod), out ignored))
                    .OutRef(typeof(GradingPeriodReference))
                    .Return(true);
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var resource = new GradingPeriod()
                {
                    beginCalendarDateReference = new CalendarDateReference
                    {
                        educationOrganizationId = 1234,
                        date = DateTime.Today.AddDays(-1)
                    },
                    endCalendarDateReference = new CalendarDateReference
                    {
                        educationOrganizationId = 1234,
                        date = DateTime.Today.AddDays(1)
                    },
                    descriptor = "12",
                };

                var resourceReferenceFactory = new ResourceReferenceFactory(_apiSdkReflectionProvider);
                _actualReference = (GradingPeriodReference) resourceReferenceFactory.CreateResourceReference(resource);
            }

            [Assert]
            public void Should_populate_the_non_role_named_properties_from_reference()
            {
                _actualReference.educationOrganizationId.ShouldEqual(1234);
            }

            [Assert]
            public void Should_populate_the_non_role_named_properties_from_resource()
            {
                _actualReference.descriptor.ShouldEqual("12");
            }

            [Assert]
            public void Should_determine_role_name_on_reference_property_and_set_the_target_reference_corresponding_property_value()
            {
                _actualReference.beginDate.ShouldEqual(DateTime.Today.AddDays(-1));
            }
        }

        public class When_creating_a_reference_from_an_instance_that_has_reference_that_uses_convention_where_containing_type_name_appears_as_prefix_and_is_trimmed : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private SectionReference _actualReference;

            // External dependencies
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                Type ignored;
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(x => x.TryGetReferenceType(typeof(Section), out ignored))
                    .OutRef(typeof(SectionReference))
                    .Return(true);
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var resource = new Section()
                {
                    classPeriodReference = new ClassPeriodReference
                    {
                        schoolId = 1234,
                        name = "Period1",
                    },
                };

                var resourceReferenceFactory = new ResourceReferenceFactory(_apiSdkReflectionProvider);
                _actualReference = (SectionReference)resourceReferenceFactory.CreateResourceReference(resource);
            }

            [Assert]
            public void Should_correctly_identify_the_secondary_reference_use_of_the_type_name_as_trimmed_property_prefix_naming_convention()
            {
                _actualReference.classPeriodName.ShouldEqual("Period1");
            }
        }
        
        public class When_creating_a_reference_from_an_instance_that_has_an_identifier_property_prefixed_with_the_class_name : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private LearningStandardReference _actualReference;

            // External dependencies
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                Type ignored;
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(x => x.TryGetReferenceType(typeof(LearningStandard), out ignored))
                    .OutRef(typeof(LearningStandardReference))
                    .Return(true);
            }

            protected override void Act()
            {
                // Perform the action to be tested
                var resource = new LearningStandard()
                {
                    id = "Not this one",
                    learningStandardId = "This One",
                };

                var resourceReferenceFactory = new ResourceReferenceFactory(_apiSdkReflectionProvider);
                _actualReference = (LearningStandardReference)resourceReferenceFactory.CreateResourceReference(resource);
            }

            [Assert]
            public void Should_correctly_identify_the_secondary_reference_use_of_the_type_name_as_trimmed_property_prefix_naming_convention()
            {
                _actualReference.learningStandardId.ShouldEqual("This One");
            }
        }

        #region SDK classes for multiple potential matches test
       
        public class AssessmentReference
        {
            /* The title or name of the assessment.  NEDM: Assessment Title   */
            public string title { get; set; }
        }

        public class Assessment
        {
            /* A reference to the related AssessmentFamily resource. */
            public AssessmentFamilyReference assessmentFamilyReference { get; set; }

            /* The title or name of the assessment.  NEDM: Assessment Title   */
            public string title { get; set; }

            /* This entity represents a tool, instrument, process or exhibition composed of a systematic sampling of behavior for measuring a student's competence, knowledge, skills or behavior. An assessment can be used to measure differences in individuals or groups and changes in performance from one occasion to the next. */
            public AssessmentContentStandard contentStandard { get; set; }        
        }

        public class AssessmentContentStandard
        {
            /* The name of the content standard, for example Common Core. */
            public string title { get; set; }
        }

        public class AssessmentFamilyReference
        {
            /* The title or name of the assessment family. */
            public string title { get; set; }
        }

        #endregion

        public class When_creating_a_reference_from_an_instance_that_has_a_property_that_has_multiple_potential_matches_due_to_use_of_the_containing_type_name_as_property_prefix_trimming_convention : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private AssessmentReference _actualReference;

            // External dependencies
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                Type ignored;
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(x => x.TryGetReferenceType(typeof(Assessment), out ignored))
                    .OutRef(typeof(AssessmentReference))
                    .Return(true);
            }

            protected override void Act()
            {
                // Set up a model with multiple potential sources, based on context/convention
                var resource = new Assessment()
                {
                    title = "This one",
                    assessmentFamilyReference = new AssessmentFamilyReference
                    {
                        title = "Not this one"
                    },
                    contentStandard = new AssessmentContentStandard
                    {
                        title = "Not this one either",
                    }
                };

                var resourceReferenceFactory = new ResourceReferenceFactory(_apiSdkReflectionProvider);
                _actualReference = (AssessmentReference) resourceReferenceFactory.CreateResourceReference(resource);
            }

            [Assert]
            public void Should_prioritize_the_property_from_the_main_resource()
            {
                _actualReference.title.ShouldEqual("This one");
            }
        }

        public class Grade
        {
            /* A reference to the related GradingPeriod resource. */
            public GradingPeriodReference gradingPeriodReference { get; set; }

            /* A reference to the related StudentSectionAssociation resource. */
            public StudentSectionAssociationReference studentSectionAssociationReference { get; set; }
        }

        public class GradeReordered
        {
            /* The unique identifier of the resource. */
            public string id { get; set; }

            /* A reference to the related StudentSectionAssociation resource. */
            public StudentSectionAssociationReference studentSectionAssociationReference { get; set; }

            /* A reference to the related GradingPeriod resource. */
            public GradingPeriodReference gradingPeriodReference { get; set; }
        }

        public class StudentSectionAssociationReference
        {
            /* Month, day and year of the student''s entry or assignment to the section.  If blank, default is the start date of the first grading period.  NEDM: EntryDate   */
            public DateTime? beginDate { get; set; }
        }

        public class GradeReference
        {
            /* Month, day and year of the student's entry or assignment to the section. */
            public DateTime? beginDate { get; set; }

            /* Month, day, and year of the first day of the grading period. */
            public DateTime? gradingPeriodBeginDate { get; set; }
        }

        #region Log file of the problem
        // Source (resource) object:
        //2014-11-23 17:53:58,139 [1] DEBUG EdFi.Ods.Utilities.LoadGeneration.ApiSdkFacade [(null)] - POST message body for 'Grade':
        //{
        //  "id": "5a067b74060e43509c7d3e3b4a7ce955",
        //  "gradingPeriodReference": {
        //    "educationOrganizationId": 10,
        //    "descriptor": "04",
        //    "beginDate": "2004-11-23T00:00:00",       <<-------------- should go to grading period begin date
        //    "link": null
        //  },
        //  "studentSectionAssociationReference": {
        //    "studentUniqueId": "T37TETZWS",
        //    "schoolId": 100025,
        //    "classPeriodName": "9-String-71xxxxxxxxx",
        //    "classroomIdentificationCode": "J-String-81xxxxxxxxx",
        //    "localCourseCode": "A-String-72xxxxxxxxx",
        //    "termType": "Fall Semester",
        //    "schoolYear": 2015,
        //    "beginDate": "2004-12-19T00:00:00",       <<-------------- should go to begin date
        //    "link": null
        //  },
        //  "type": "Conduct",
        //  "letterGradeEarned": "5-String-67xxxxxxxxx",
        //  "numericGradeEarned": 12,
        //  "diagnosticStatement": "6-String-68xxxxxxxxx",
        //  "performanceBaseConversionType": "Advanced",
        //  "_etag": null
        //}

        // Target (reference) object:
        //2014-11-23 17:53:58,156 [1] DEBUG EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders.ReportCardGrade_GradeReference_ValueBuilder [(null)] - Finished creating/persisting 'Grade' for reference 'GradeReference' [correlationId=3f68977d7e324e16bc2afb60b847f63d].
        //2014-11-23 17:53:58,178 [1] DEBUG EdFi.Ods.Utilities.LoadGeneration.ContextSpecificReferenceValueBuilder [(null)] - Value builder 'ContextSpecificReferenceValueBuilder' handled the request for property path 'ReportCard.grades[0].ReportCardGrade.gradeReference' and built a value of '
        //{
        //  "studentUniqueId": "T37TETZWS",
        //  "schoolId": 100025,
        //  "classPeriodName": "9-String-71xxxxxxxxx",
        //  "classroomIdentificationCode": "J-String-81xxxxxxxxx",
        //  "localCourseCode": "A-String-72xxxxxxxxx",
        //  "termType": "Fall Semester",
        //  "schoolYear": 2015,
        //  "beginDate": "2004-11-23T00:00:00",         <<---------------- set to wrong begin date
        //  "gradingPeriodDescriptor": "04",
        //  "gradingPeriodBeginDate": null,             <<---------------- left null
        //  "gradingPeriodEducationOrganizationId": 10,
        //  "type": "Conduct",
        //  "link": null
        //}'.

        #endregion

        public class When_gathering_values_from_the_grade_resource_where_there_are_multiple_potential_sources_for_the_begin_date_properties : TestFixtureBase
        {
            // Supplied values

            // Actual values
            private GradeReference _actualReference;
            private GradeReference _actualReferenceFromReordered;

            // External dependencies
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;

            protected override void Arrange()
            {
                // Set up mocked dependences and supplied values
                Type ignored;
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(x => x.TryGetReferenceType(typeof(Grade), out ignored))
                    .OutRef(typeof(GradeReference))
                    .Return(true);

                // For building with an alternate order (just to make sure we've fully covered the possible scenario)
                _apiSdkReflectionProvider.Expect(x => x.TryGetReferenceType(typeof(GradeReordered), out ignored))
                    .OutRef(typeof(GradeReference))
                    .Return(true);
            }

            protected override void Act()
            {
                // Set up a model with multiple potential sources, based on context/convention
                var resource = new Grade()
                {
                    gradingPeriodReference = new GradingPeriodReference()
                    {
                        beginDate = DateTime.Today,
                    },
                    studentSectionAssociationReference = new StudentSectionAssociationReference
                    {
                        beginDate = DateTime.Today.AddDays(1),
                    }
                };

                var resourceReferenceFactory = new ResourceReferenceFactory(_apiSdkReflectionProvider);
                _actualReference = (GradeReference) resourceReferenceFactory.CreateResourceReference(resource);

                var resourceReordered = new GradeReordered()
                {
                    gradingPeriodReference = new GradingPeriodReference()
                    {
                        beginDate = DateTime.Today,
                    },
                    studentSectionAssociationReference = new StudentSectionAssociationReference
                    {
                        beginDate = DateTime.Today.AddDays(1),
                    }
                };

                _actualReferenceFromReordered = (GradeReference)resourceReferenceFactory.CreateResourceReference(resourceReordered);
            }

            [Assert]
            public void Should_assign_grading_period_begin_date_from_the_grading_period_reference()
            {
                // Assert the expected results
                _actualReference.gradingPeriodBeginDate.ShouldEqual(DateTime.Today);
            }

            [Assert]
            public void Should_assign_begin_date_from_the_other_reference_not_using_a_role_name_prefix_convention()
            {
                // Assert the expected results
                _actualReference.beginDate.ShouldEqual(DateTime.Today.AddDays(1));
            }

            [Assert]
            public void Should_assign_grading_period_begin_date_from_the_grading_period_reference_even_if_the_order_of_the_references_happens_to_be_reversed()
            {
                // Assert the expected results
                _actualReferenceFromReordered.gradingPeriodBeginDate.ShouldEqual(DateTime.Today);
            }

            [Assert]
            public void Should_assign_begin_date_from_the_other_reference_not_using_a_role_name_prefix_convention_even_if_the_order_of_the_references_happens_to_be_reversed()
            {
                // Assert the expected results
                _actualReferenceFromReordered.beginDate.ShouldEqual(DateTime.Today.AddDays(1));
            }
        }


        [TestFixture]
        public class When_creating_a_reference_from_an_instance : TestFixtureBase
        {
            private SomeClass _suppliedInstance;
            private IApiSdkReflectionProvider _apiSdkReflectionProvider;
            private IResourceReferenceFactory _resourceReferenceFactory;
            private SomeClassReference _createdReference;
            private Exception _actualException;

            protected override void Arrange()
            {
                Type ignored;
                _apiSdkReflectionProvider = Stub<IApiSdkReflectionProvider>();
                _apiSdkReflectionProvider.Expect(x => x.TryGetReferenceType(typeof(SomeClass), out ignored))
                    .OutRef(typeof(SomeClassReference))
                    .Return(true);
            }

            protected override void Act()
            {
                _suppliedInstance = new SomeClass()
                {
                    AnotherClassReference = new AnotherClassReference()
                    {
                        ReferenceDateTimeProperty = new DateTime(1999, 9, 9),
                    },
                    AnOptionalReference = null,             // Property searching should property avoid null references
                    StringBuilder = new StringBuilder(10), // Property searching should property avoid .NET classes (otherwise Capacity = 10)
                    ReferenceIntProperty = 999,
                    ReferenceStringProperty = "Nine",
                    UnneededDecimal = 123.45m,
                    UnneededString = "Unneeded",
                };

                _resourceReferenceFactory = new ResourceReferenceFactory(_apiSdkReflectionProvider);

                _createdReference = (SomeClassReference) _resourceReferenceFactory.CreateResourceReference(_suppliedInstance);
            }

            [Assert]
            public void Should_gather_all_values_present_on_the_instance()
            {
                _createdReference.ReferenceIntProperty.ShouldEqual(999);
                _createdReference.ReferenceStringProperty.ShouldEqual("Nine");
            }

            [Assert]
            public void Should_gather_values_from_object_references()
            {
                _createdReference.ReferenceDateTimeProperty.ShouldEqual(new DateTime(1999, 9, 9));
            }

            [Assert]
            public void Should_not_process_dotnet_classes()
            {
                _createdReference.Capacity.ShouldNotEqual(10);
            }
        }
    }
}
