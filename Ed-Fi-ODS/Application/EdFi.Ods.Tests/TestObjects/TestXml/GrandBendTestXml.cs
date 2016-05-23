using System.Collections.Generic;
using System.IO;
using EdFi.Ods.Common.Utils.Resources;

namespace EdFi.Ods.Tests.TestObjects.TestXml
{
    public static class GrandBendTestXml
    {
        public const string IdForProgramWithEdOrgIdentityElement = "PRGM_2559011";
        public const string ReferencedEdOrgIdInProgram = "255901";
        public static string IdForSchoolWithRefIdToLEAElement = "SCOL_255901107";
        public static string ReferencedLEAIdInSchool = "255901";

        public static IEnumerable<string> EdOrgAggregateRootNames = new List<string>
        {
            "StateEducationAgency",
            "EducationServiceCenter",
            "FeederSchoolAssociation",
            "LocalEducationAgency",
            "School",
            "Location",
            "ClassPeriod",
            "Course",
            "Program",
            "AccountabilityRating",
            "EducationOrganizationPeerAssociation",
            "EducationOrganizationNetwork",
            "EducationOrganizationNetworkAssociation"
        };

        public static Stream EdOrg
        {
            get
            {
                return
                    EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>(
                        "GrandBendEducationOrganizationSample.xml");
            }
        }

        public static Stream EdOrgCalendar
        {
            get
            {
                return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("GrandBendEdOrgCalendar.xml");
            }
        }

        public static IEnumerable<string> EdOrgCalendarAggregateRootNames = new List<string>
        {
            "Session",
            "GradingPeriod",
            "CalendarDate",
            "AcademicWeek"
        };

        public static Stream Standards
        {
            get
            {
                return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("GrandBendStandardSample.xml");
            }
        }

        public static IEnumerable<string> StandardsRootNames
        {
            get { return new List<string> {"LearningStandard", "LearningObjective"}; }
        }

        public static Stream StudentParent
        {
            get
            {
                return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("GrandBendStudentParent.xml");
            }
        }

        public static IEnumerable<string> StudentParentRootNames = new List<string>
        {
            "Student",
            "Parent",
            "StudentParentAssociation"
        };

        public static Stream MasterSchedule
        {
            get
            {
                return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("GrandBendMasterSchedule.xml");
            }
        }

        public static IEnumerable<string> MasterScheduleRootNames = new List<string>
        {
            "CourseOffering",
            "Section",
            "BellSchedule"
        };

        public static Stream StaffAssociation
        {
            get
            {
                return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("GrandBendStaffAssociation.xml");
            }
        }

        public static IEnumerable<string> StaffAssociationRootNames = new List<string>
        {
            "Staff",
            "StaffEducationOrganizationEmploymentAssociation",
            "StaffEducationOrganizationAssignmentAssociation",
            "StaffSchoolAssociation",
            "StaffSectionAssociation",
            "LeaveEvent",
            "OpenStaffPosition",
            "StaffProgramAssociation"
        };

        public static Stream StudentProgram
        {
            get
            {
                return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("GrandBendStudentProgram.xml");
            }
        }

        public static IEnumerable<string> StudentProgramRootNames = new[]
        {
            "StudentProgramAssociation",
            "StudentSpecialEducationProgramAssociation",
            "RestraintEvent",
            "StudentCTEProgramAssociation",
            "StudentTitleIPartAProgramAssociation",
            "StudentMigrantEducationProgramAssociation"
        };

        public static Stream StudentEnrollment
        {
            get
            {
                return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("GrandBendStudentEnrollment.xml");
            }
        }

        public static IEnumerable<string> StudentEnrollmentRootNames = new[]
        {
            "StudentSchoolAssociation",
            "StudentSectionAssociation",
            "GraduationPlan",
            "StudentEducationOrganizationAssociation"
        };

        public static Stream StudentDiscipline
        {
            get
            {
                return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("GrandBendStudentDiscipline.xml");
            }
        }

        public static IEnumerable<string> StudentDisciplineRootNames = new[]
        {
            "DisciplineIncident",
            "StudentDisciplineIncidentAssociation",
            "DisciplineAction"
        };

        public static Stream StudentAssessment
        {
            get
            {
                return EmbeddedResourceReader.GetResourceStream<IMarkWhereTestXmlLives>("GrandBendStudentAssessment.xml");
            }
        }

        public static IEnumerable<string> StudentAssessmentRootNames = new[]
        {
            "StudentAssessment"
        };
    }
}