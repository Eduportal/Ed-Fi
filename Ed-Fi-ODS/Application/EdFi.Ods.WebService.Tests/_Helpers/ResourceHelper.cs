using System.Globalization;
using EdFi.Ods.Api.Models.Resources.Assessment;
using EdFi.Ods.Api.Models.Resources.AssessmentFamily;
using EdFi.Ods.Api.Models.Resources.EducationOrganization;
using EdFi.Ods.Api.Models.Resources.Parent;
using EdFi.Ods.Api.Models.Resources.Program;
using EdFi.Ods.Api.Models.Resources.School;
using EdFi.Ods.Api.Models.Resources.Staff;
using EdFi.Ods.Api.Models.Resources.StaffEducationOrganizationEmploymentAssociation;
using EdFi.Ods.Api.Models.Resources.Student;
using EdFi.Ods.Api.Models.Resources.StudentSchoolAssociation;
using Newtonsoft.Json;
using System;

namespace EdFi.Ods.WebService.Tests._Helpers
{
    internal class ResourceHelper
    {
        public static string CreateStudent(string uniqueId, string lastName = null, string firstName = null)
        {
            var ticks = DateTime.Now.Ticks;
            var student = new Student
            {
                StudentUniqueId = uniqueId,
                FirstName = firstName ?? string.Format("F{0}", ticks),
                LastSurname = lastName ?? string.Format("L{0}", ticks),
                SexType = ticks % 2 == 0 ? "Male" : "Female",
                BirthDate = DateTime.Now.AddYears(-10)
            };

            return JsonConvert.SerializeObject(student);
        }

        public static string CreateStudentSchoolAssociation(string uniqueId, int schoolId, string gradeLevelDescriptor = "Fourth grade")
        {
            var association = new StudentSchoolAssociation
            {
                StudentReference = new StudentReference { StudentUniqueId = uniqueId },
                SchoolReference = new SchoolReference { SchoolId = schoolId },
                EntryDate = DateTime.Today.AddYears(-1),
                EntryGradeLevelDescriptor = gradeLevelDescriptor
            };

            return JsonConvert.SerializeObject(association);
        }

        public static string CreateAssessment(string assessmentTitle, string assessmentFamilyTitle, string assessmentNamespace)
        {
            var assessment = new Assessment
            {
                AssessmentTitle = assessmentTitle,
                AcademicSubjectDescriptor = "Mathematics",
                AssessedGradeLevelDescriptor = "Third grade",
                Version = 1,
                Namespace = assessmentNamespace,
                AssessmentFamilyReference = !string.IsNullOrWhiteSpace(assessmentFamilyTitle) ? new AssessmentFamilyReference { AssessmentFamilyTitle = assessmentFamilyTitle } : null,
            };

            return JsonConvert.SerializeObject(assessment);
        }

        public static string CreateAssessmentFamily(string assessmentFamilyTitle, string assessmentFamilyNamespace)
        {
            var assessmentFamily = new AssessmentFamily
            {
                AssessmentFamilyTitle = assessmentFamilyTitle,
                Namespace = assessmentFamilyNamespace,
            };

            return JsonConvert.SerializeObject(assessmentFamily);
        }

        public static string CreateParent(string uniqueId, string lastName = null, string firstName = null)
        {
            var ticks = DateTime.Now.Ticks;
            var parent = new Parent
            {
                ParentUniqueId = uniqueId,
                FirstName = firstName ?? string.Format("F{0}", ticks),
                LastSurname = lastName ?? string.Format("L{0}", ticks),
                SexType = ticks % 2 == 0 ? "Male" : "Female",
            };

            return JsonConvert.SerializeObject(parent);
        }

        public static string CreateProgram(int educationOrganizationId)
        {
            var program = new Program
            {
                EducationOrganizationReference = new EducationOrganizationReference { EducationOrganizationId = educationOrganizationId },
                ProgramType = "Athletics",
                ProgramSponsorType = "School",
                ProgramName = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture),
                ProgramId = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)
            };

            return JsonConvert.SerializeObject(program);
        }

        public static string CreateStaff(string uniqueId, string lastName = null, string firstName = null)
        {
            var ticks = DateTime.Now.Ticks;
            var staff = new Staff
            {
                StaffUniqueId = uniqueId,
                FirstName = firstName ?? string.Format("F{0}", ticks),
                LastSurname = lastName ?? string.Format("L{0}", ticks),
                SexType = ticks % 2 == 0 ? "Male" : "Female",
            };

            return JsonConvert.SerializeObject(staff);
        }

        public static string CreateStaffEducationOrganizationEmploymentAssociation(string staffUniqueId, int educationOrganizationId)
        {
            var association = new StaffEducationOrganizationEmploymentAssociation
            {
                StaffReference = new StaffReference { StaffUniqueId = staffUniqueId },
                EducationOrganizationReference = new EducationOrganizationReference { EducationOrganizationId = educationOrganizationId },
                EmploymentStatusDescriptor = "Tenured or permanent",
                HireDate = DateTime.Now.AddYears(-2)
            };

            return JsonConvert.SerializeObject(association);
        }
    }
}
