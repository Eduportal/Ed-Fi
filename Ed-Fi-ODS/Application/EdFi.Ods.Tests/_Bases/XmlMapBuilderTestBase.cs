namespace EdFi.Ods.Tests._Bases
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    using Newtonsoft.Json;

    public class XmlMapBuilderTestBase : SchemaMetadataTestBase
    {
        protected static string STAFFPROGRAMASSOCIATION = "StaffProgramAssociation";
        protected static string EDUCATIONORGANIZATIONID = "EducationOrganizationId";
        protected static string PROGRAMTYPE = "ProgramType";
        protected static string PROGRAMREFERENCE = "ProgramReference";
        protected static string STRINGWITHSPECIALCHARACTERS = @"a_serialized""/\_map";

        private Dictionary<Tuple<string, string>, Func<string>> _serializedMapsDictionary;

        public XmlMapBuilderTestBase()
        {
            this._serializedMapsDictionary = new Dictionary<Tuple<string, string>, Func<string>>();
            this._serializedMapsDictionary.Add(new Tuple<string, string>(STAFFPROGRAMASSOCIATION, PROGRAMTYPE), this.SerializedMapToProgramTypeInStaffProgramAssociation);
            this._serializedMapsDictionary.Add(new Tuple<string, string>(STAFFPROGRAMASSOCIATION, EDUCATIONORGANIZATIONID), this.SerializedMapToEdOrgId);
        }

        protected string SerializedMapFor(string startingXsd, string targetElement)
        {
            var key = new Tuple<string, string>(startingXsd, targetElement);
            return this._serializedMapsDictionary.ContainsKey(key) ? this._serializedMapsDictionary[key]() : string.Empty;
        }

        private string SerializedMapToEdOrgId()
        {
            var refStep = new RawStep {ElementName = "ProgramReference", IsReference = true, ReferenceTarget = "Program"};
            var EdOrgRefStep = new RawStep
            {
                ElementName = "EducationOrganizationReference",
                ParentElement = "ProgramIdentity",
                IsReference = true,
                ReferenceTarget = "EducationOrganization",
                Parent = refStep,
                IsEdOrgRef = true
            };
            var stepToEdOrgId = new RawStep
            {
                ElementName = EDUCATIONORGANIZATIONID,
                ParentElement = "EducationOrganizationIdentity",
                Parent = EdOrgRefStep
            };
            return JsonConvert.SerializeObject(stepToEdOrgId);
        }

        private string SerializedMapToProgramTypeInStaffProgramAssociation()
        {
            var refStep = new RawStep {ElementName = "ProgramReference", IsReference = true, ReferenceTarget = "Program"};
            var stepToProgramType = new RawStep
            {
                ElementName = PROGRAMTYPE,
                ParentElement = "ProgramIdentity",
                Parent = refStep
            };
            return JsonConvert.SerializeObject(stepToProgramType);
        }

        internal class RawStep
        {
            public bool IsReference { get; set; }
            public string ReferenceTarget { get; set; }
            public RawStep Parent { get; set; }
            public string ElementName { get; set; }
            public string ParentElement { get; set; }
            public bool IsEdOrgRef { get; set; }

            public bool HasParent()
            {
                return this.Parent != null;
            }
        }

        protected string FindSerializedMapTo(string element, IEnumerable<Tuple<ParsedSchemaObject, string>> tuples)
        {
            return tuples.Any(t => t.Item2.Contains(element)) ? tuples.Single(t => t.Item2.Contains(element)).Item2 : string.Empty;
        }

        protected ParsedSchemaObject FindReturnedObjectTo(string targetElement,
            IEnumerable<Tuple<ParsedSchemaObject, string>> tuples)
        {
            return tuples.Any(t => t.Item2.Contains(targetElement))
                ? tuples.Single(t => t.Item2.Contains(targetElement)).Item1
                : null;
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


        protected IMapStep ProgramTypeInStaffProgramMap()
        {
            var StepToProgramTypeInProgramRef = new ElementStep(new[] {"ProgramIdentity", "ProgramType"}, null);
            var programRefDictionary = new Dictionary<string, IMapStep> {{"ProgramReference",StepToProgramTypeInProgramRef}};
            return new ReferenceStep(new []{"ProgramReference"}, StepToProgramTypeInProgramRef, programRefDictionary);
        }

    }
}