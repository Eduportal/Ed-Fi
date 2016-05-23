namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.XmlMapBuilderTests
{
    using System;
    using System.Linq;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.CodeGen.XsdToWebApi.Process;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class Single_Target_Tests : XmlMapBuilderTestBase
    {
        [Test]
        public void
            When_Building_Maps_For_EdOrg_Reference_In_StaffEducationOrganizationAssignmentAssociation_Should_Identify_It_As_Int32
            ()
        {
            var name = "StaffEducationOrganizationAssignmentAssociation";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName == name);
            var sut = new XPathMapBuilder();
            var tuples = sut.BuildStartingXsdElementSerializedMapTuplesFor(xsdObject);
            var mappedXsds =
                tuples.Where(t => t.Item2.Contains("EducationOrganizationReference")).Select(t => t.Item1).ToList();
            mappedXsds.All(x =>
                           ((ExpectedTerminalRestProperty) x.ProcessResult.Expected).PropertyType == typeof (Int32)
                           || ((ExpectedTerminalRestProperty) x.ProcessResult.Expected).PropertyType == typeof (Int32?)).ShouldBeTrue();
        }

        [Test]
        [Ignore("This test fails because the xsd Parser in Xsd-to-Api incorrectly identifies the object - once fixed this test should pass")]
        public void Given_Object_With_Nested_Keys_Should_Return_Tuple_For_Nested_Keys()
        {
            var name = "StaffEducationOrganizationEmploymentAssociation";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName == name);
            var sut = new XPathMapBuilder();
            var tuples = sut.BuildStartingXsdElementSerializedMapTuplesFor(xsdObject);
            var nestedFK =
                tuples.Where(t => t.Item1.ParentElement.IsCommonExpansion()).Select(t => t.Item1).Single();
            nestedFK.ShouldNotBeNull();
        }

        [Test]
        public void Given_Nested_Reference_Element_That_Appears_In_Multiple_Elements_When_Map_Built_Then_Should_Build_Maps_To_Each_Tree()
        {
            //This test assumes the LocationReference will be before the ClassPeriodReference in the Xsd and therefore would 'hide' the classperiod school id if we let it
            var name = "StaffSectionAssociation";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName == name);
            var sut = new XPathMapBuilder();
            var tuples = sut.BuildStartingXsdElementSerializedMapTuplesFor(xsdObject);
            var classPeriodSchoolRef =
                tuples.Single(t => t.Item1.ParentElement.ParentElement.ParentElement.XmlSchemaObjectName == "ClassPeriodIdentity" && t.Item1.XmlSchemaObjectName == "SchoolId");
            var map = sut.DeserializeMap(classPeriodSchoolRef.Item2);
            map.GetXPath()[0].ShouldEqual("SectionReference");
            var second = map.GetNextStep();
            second.GetXPath()[0].ShouldEqual("ClassPeriodReference");
            var third = second.GetNextStep();
            third.GetXPath()[0].ShouldEqual("SchoolReference");
            var fourth = third.GetNextStep();
            fourth.GetXPath()[0].ShouldEqual("SchoolId");
            fourth.IsTerminal().ShouldBeTrue();
        }

        [Test]
        public void Given_Session_Xsd_Should_Identify_GradingPeriod_As_Target_For_All_Contained_Keys()
        {
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName == "Session");
            xsdObject = xsdObject.ChildElements.Single(c => c.XmlSchemaObjectName == "GradingPeriodReference");
            var sut = new XPathMapBuilder();
            var tuples = sut.BuildStartingXsdElementSerializedMapTuplesFor(xsdObject);
            var serializedMap = tuples.First(t => t.Item2.Contains("BeginDate")).Item2;
            Console.WriteLine(serializedMap);
            var map = sut.DeserializeMap(serializedMap);
            ((IReferenceStep) map).ReferencePointedToAggregateType("GradingPeriod", string.Empty);
            var terminal = map.GetNextStep();
            terminal.GetXPath()[0].ShouldEqual("BeginDate");
        }

        [Test]
        public void Get_Assessment_Item_Assessed_Grade_Level_Path()
        {
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName == "AssessmentItem");
            xsdObject = xsdObject.ChildElements.Single(c => c.XmlSchemaObjectName == "AssessmentReference");
            var mapBuilder = new XPathMapBuilder();
            var tuples = mapBuilder.BuildStartingXsdElementSerializedMapTuplesFor(xsdObject);
            var serializedMap = tuples.First(t => t.Item2.Contains("AssessedGradeLevel")).Item2;
            Console.WriteLine(serializedMap);
            var map = mapBuilder.DeserializeMap(serializedMap);
            ((IReferenceStep) map).ReferencePointedToAggregateType("Assessment", string.Empty);
            var terminal = map.GetNextStep();
            terminal.GetXPath()[0].ShouldEqual("AssessedGradeLevel");
            terminal.GetXPath()[1].ShouldEqual("CodeValue");
        }

        [Test]
        public void Should_get_classroomItendificationCode_from_location_from_section_from_gradebookEntry_from_studentGradebookEntry()
        {
            var xpathMapBuilder = new XPathMapBuilder();

            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName == "StudentGradebookEntry");
            
            //Make sure we're dealing with the ClassroomIdentificationCode from GradebookEntry -> Section -> Location
            xsdObject = xsdObject.ChildElements.Single(x => x.XmlSchemaObjectName == "GradebookEntryReference");
            var tuples = xpathMapBuilder.BuildStartingXsdElementSerializedMapTuplesFor(xsdObject);
            
            //Use .Single() to make sure there is only one.
            var classroomIdCodeMap = tuples.Single(x => x.Item2.Contains("ClassroomIdentificationCode")).Item2;

            Console.WriteLine(classroomIdCodeMap);

            var classroomIdCodeReferenceStep = xpathMapBuilder.DeserializeMap(classroomIdCodeMap);
            ((IReferenceStep)classroomIdCodeReferenceStep).ReferencePointedToAggregateType("GradebookEntryReference", string.Empty);
            var sectionReferenceStep = classroomIdCodeReferenceStep.GetNextStep();
            ((IReferenceStep)sectionReferenceStep).ReferencePointedToAggregateType("SectionReference", string.Empty);
            var locationReferenceStep = sectionReferenceStep.GetNextStep();
            ((IReferenceStep)locationReferenceStep).ReferencePointedToAggregateType("LocationReference", string.Empty);
            var classroomIdCodeStep = locationReferenceStep.GetNextStep();
            classroomIdCodeStep.IsTerminal().ShouldBeTrue();
        }
    }
}