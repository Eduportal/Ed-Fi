namespace EdFi.Ods.Tests._Bases
{
    using System.Collections.Generic;

    using global::EdFi.Ods.BulkLoad.Core;
    using global::EdFi.Ods.CodeGen.XmlShredding;

    public class XmlFileTestBase : TestBase
    {
        protected IMapStep GetSchoolMap()
        {
            var StepToEdOrgIdFromLEA = new ElementStep(new []{"StateOrganizationId[1]"}, null);
            var refDictionary = new Dictionary<string, IMapStep>();
            refDictionary.Add("LocalEducationAgency", StepToEdOrgIdFromLEA);
            var stepToLEAIdFromSchool = new ElementStep(new[] {"StateOrganizationId[1]"}, null);
            return new ReferenceStep(new[] {"LocalEducationAgencyReference[1]"}, stepToLEAIdFromSchool, refDictionary);
        }

        protected IMapStep GetProgramMap()
        {
            var StepToEdOrgIdFromESC = new ElementStep(new[] {"StateOrganizationId[1]"}, null);
            var StepToEdOrgIdFromSchool = new ElementStep(new []{"StateOrganizationId[1]"}, null);
            var StepToEdOrgIdFromLEA = new ElementStep(new []{"StateOrganizationId[1]"}, null);
            var referencedStepDictionary = new Dictionary<string, IMapStep>();
            referencedStepDictionary.Add("LocalEducationAgency", StepToEdOrgIdFromLEA);
            referencedStepDictionary.Add("School", StepToEdOrgIdFromSchool);
            referencedStepDictionary.Add("EducationServiceCenter", StepToEdOrgIdFromESC);
            var StepToEdOrgIdFromEdOrgRef = new ElementStep(new []{"EducationOrganizationIdentity","EducationOrganizationId[1]"}, null);
            return new ReferenceStep(new []{"EducationOrganizationReference[1]"},StepToEdOrgIdFromEdOrgRef, referencedStepDictionary);
        }

        protected IMapStep EdOrgInStaffProgramMap()
        {
            var StepToEdOrgIdFromESC = new ElementStep(new[] {"StateOrganizationId[1]"}, null);
            var StepToEdOrgIdFromSchool = new ElementStep(new []{"StateOrganizationId[1]"}, null);
            var StepToEdOrgIdFromLEA = new ElementStep(new []{"StateOrganizationId[1]"}, null);
            var referencedStepDictionary = new Dictionary<string, IMapStep>();
            referencedStepDictionary.Add("LocalEducationAgency", StepToEdOrgIdFromLEA);
            referencedStepDictionary.Add("School", StepToEdOrgIdFromSchool);
            referencedStepDictionary.Add("EducationServiceCenter", StepToEdOrgIdFromESC);
            var StepToEdOrgIdFromEdOrgRef = new ElementStep(new []{"EducationOrganizationIdentity","EducationOrganizationId[1]"}, null);
            var StepToEdOrgRefFromProgramRef =
                new ReferenceStep(new[] {"ProgramIdentity", "EducationOrganizationReference[1]"},
                    StepToEdOrgIdFromEdOrgRef, referencedStepDictionary);
            var programRefDictionary = new Dictionary<string, IMapStep>();
            programRefDictionary.Add("ProgramReference",StepToEdOrgRefFromProgramRef);
            return new ReferenceStep(new []{"ProgramReference"}, StepToEdOrgRefFromProgramRef, programRefDictionary);
        }

        protected IXmlGPS GetProgramGPS(string serializedMap, string namespacePrefix)
        {
            return new XmlGPS(this.GetProgramMap(), namespacePrefix);
        }

        protected IXmlGPS GetSchoolGPS(string serializedMap, string namespacePrefix)
        {
            return new XmlGPS(this.GetSchoolMap(), namespacePrefix);
        }

        protected IXmlGPS GetSerializedMapBuilder(string map, string namespacePrefix)
        {
            if (string.IsNullOrWhiteSpace(map)) return null;
            var builder = new XPathMapBuilder();
            return new XmlGPS(builder.DeserializeMap(map), namespacePrefix);
        }

        protected string GetProgramEdOrgIdentityMap()
        {
            return string.Empty;
        }

    }
}