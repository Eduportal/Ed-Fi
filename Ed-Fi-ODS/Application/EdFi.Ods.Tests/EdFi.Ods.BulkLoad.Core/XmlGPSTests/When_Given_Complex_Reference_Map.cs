namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.XmlGPSTests
{
    using System;
    using System.Linq;

    using global::EdFi.Common.Extensions;
    using global::EdFi.Ods.BulkLoad.Core;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class When_Given_Complex_Reference_Map : XmlFileTestBase
    {
        [Test]
        public void Should_Prefix_XPaths_With_Dot_Slash_Slash_Namespace_Prefix_Colon()
        {
            var namespacePrefix = "bob";
            var expectedBeginning = string.Format(".//{0}:", namespacePrefix);
            var map = this.EdOrgInStaffProgramMap();
            var sut = new XmlGPS(map, "bob");
            var xpath = sut.GetXPathToCurrent();
            xpath.StartsWith(expectedBeginning).ShouldBeTrue();
        }

        [Test]
        public void And_XPath_Is_MultiElement_Should_Prefix_Each_Element_With_Namespace_Prefix()
        {
            var namespacePrefix = "bob";
            var separator = new[]{string.Format("/{0}:", namespacePrefix)};
            var map = this.EdOrgInStaffProgramMap();
            var sut = new XmlGPS(map, "bob");
            sut.GoToNextStep(); //The second step in this map should be multielement (if the map in the base class is altered this may be false)
            var xpath = sut.GetXPathToCurrent();
            var elements = xpath.Split(separator, StringSplitOptions.None);
            //first string will be the prefix part (see prefix test for clarification)
            elements[1].ShouldEqual("ProgramIdentity");
            elements[2].ShouldEqual("EducationOrganizationReference[1]");
        }

        [Test]
        public void And_Asked_For_Target_Types_Should_Return_All_Types_in_Map()
        {
            var map = this.GetProgramMap();
            var sut = new XmlGPS(map, "not relevant");
            var targetTypes = sut.GetReferencedAggregateTypes();
            targetTypes.Count().ShouldEqual(3);
        }

        [Test]
        public void
            And_Current_Step_Is_Reference_And_Caller_Does_Not_Indicate_Aggregate_Reference_Found_When_NextStep_Called_Should_Return_In_Reference_Step
            ()
        {
            var map = this.GetProgramMap();
            var sut = new XmlGPS(map, "not relevant");
            sut.GoToNextStep();
            var xpath = sut.GetXPathToCurrent();
            xpath.EndsWith("EducationOrganizationId[1]").ShouldBeTrue();
        }

        [Test]
        public void
            And_Current_Step_Is_Reference_And_Caller_Indicates_Aggregate_Reference_Found_When_NextStep_Called_Should_Return_Step_To_the_Aggregates_Element
            ()
        {
            var map = this.GetProgramMap();
            var sut = new XmlGPS(map, "Not Relevant");
            sut.ReferenceWasToAggregate("School");
            sut.GoToNextStep();
            var xpath = sut.GetXPathToCurrent();
            xpath.EndsWith("StateOrganizationId[1]").ShouldBeTrue();
        }

        //In all fairness, this test is terrible but was created to aid in the design of the XmlGPS 
        //Was left because this part of the codebase is abstract enough I (Gregory) felt it would aid
        //someone in understanding how the GPS and maps work - the GPS has no fore-knowledge of the maps
        //it simply navigates them according to the behavior of the caller.  It makes some assumptions
        //namely it assumes the caller is not navigating to other elements of Xml file without calling
        //GoToNextStep and it assumes the caller will call IXmlGPS.ReferenceWasToAggregate(string AggregateName)
        //when they find a Reference Id and navigate to the referenced element (which must be an Aggregate)
        [Test]
        public void And_Caller_Navigates_Full_Map_Should_Arrive_At_Terminal_Step()
        {
            var map = this.EdOrgInStaffProgramMap();
            var sut = new XmlGPS(map, "does not matter");
            sut.GoToNextStep(); //Moves to the EdOrg Reference which is the default if the Ref Id Attribute is missing or empty
            sut.GoToNextStep(); //Moves to the EdOrgId Element in the EdOrgReference Element which is the default if the Ref Id Attribute in EdOrgRef is missing or empty
            sut.CurrentStepIsTerminal().ShouldBeTrue();
        }
    }
}