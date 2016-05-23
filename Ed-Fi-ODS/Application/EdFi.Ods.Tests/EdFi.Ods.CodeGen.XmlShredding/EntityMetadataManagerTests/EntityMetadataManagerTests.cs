namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.XmlShredding.EntityMetadataManagerTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    [TestFixture]
    public class EntityMetadataManagerTests : SchemaMetadataTestBase
    {
        [Test]
        public void When_Given_an_Entity_Should_Determine_Name()
        {
            var name = "AcademicSubjectDescriptor";
            var parsedObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var sut = new EntityMetadataManager(parsedObject, this.Stub<IXPathMapBuilder>());
            sut.EntityName.ShouldEqual(name);
        }

        [Test]
        public void When_Given_an_Entity_Should_Determine_Resource_Namespace()
        {
            var pname = "School";
            var root = this.GetParsed(x => x.XmlSchemaObjectName.Equals(pname));
            var name = "GradeLevel";
            var child = root.ChildElements.Find(c => c.XmlSchemaObjectName.Contains(name));
            var expectedResourceNamespace = "EdFi.Ods.Api.Models.Resources." + pname;
            var sut = new EntityMetadataManager(child, this.Stub<IXPathMapBuilder>());
            sut.ResourceNamespace.ShouldEqual(expectedResourceNamespace);
        }


        [Test]
        public void When_Given_an_Aggregate_Entity_Should_Determine_Resource_Namespace()
        {
            var name = "AcademicSubjectDescriptor";
            var expectedResourceNamespace = "EdFi.Ods.Api.Models.Resources." + name;
            var parsedObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var sut = new EntityMetadataManager(parsedObject, this.Stub<IXPathMapBuilder>());
            sut.ResourceNamespace.ShouldEqual(expectedResourceNamespace);
        }

        [Test]
        public void When_Given_an_Aggregate_Should_Determine_EntityInterface()
        {
            var name = "GradeLevelDescriptor";
            var parsedObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var interfaceName = "I" + name;
            var sut = new EntityMetadataManager(parsedObject, this.Stub<IXPathMapBuilder>());
            sut.EntityInterface.ShouldEqual(interfaceName);
        }

        [Test]
        public void When_Given_An_Entity_With_At_Least_One_Single_Value_Property_Should_Generate_SimplePropertyObject()
        {
            var name = "EducationServiceCenter";
            var propertyName = "NameOfInstitution";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var sut = new EntityMetadataManager(xsdObject, this.Stub<IXPathMapBuilder>());
            var simpleProperties = sut.GetSingleSimpleTypedProperties();
            var result = simpleProperties.First(p => p.PropertyName.Equals(propertyName));
            result.ShouldNotBeNull();
            var expectedType = typeof (string);
            result.PropertyType.ShouldEqual(expectedType);
            result.ElementName.ShouldEqual(propertyName);
        }

        [Test]
        public void When_Given_An_Entity_With_A_Type_Property_Should_Recognize_As_Simple_Property_Type()
        {
            var name = "AcademicSubjectDescriptor";
            var parsedObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var sut = new EntityMetadataManager(parsedObject, this.Stub<IXPathMapBuilder>());
            var simpleTypes = sut.GetSingleSimpleTypedProperties();
            var typeProperty = simpleTypes.FirstOrDefault(p => p.PropertyName == "AcademicSubjectType");
            typeProperty.PropertyName.ShouldEqual("AcademicSubjectType");
        }

        [Test]
        public void
            When_Given_An_Entity_That_Is_An_Extended_Descriptor_Reference_Should_Correctly_Identify_The_Interface()
        {
            var ename = "LevelDescriptor";
            var entity = this.GetParsed(o => o.XmlSchemaObjectName == ename);
            var parentEMM = new EntityMetadataManager(entity, this.Stub<IXPathMapBuilder>());
            var sut = parentEMM.GetEntityTypedCollectionProperties().Single().GetMetaDataMgr();
            sut.EntityInterface.ShouldEqual("ILevelDescriptorGradeLevel");
            sut.EntityName.ShouldEqual("LevelDescriptorGradeLevel");
        }

        [Test]
        public void When_Given_An_Entity_With_An_Extended_Descriptor_Reference_Should_Return_It_As_Entity_Collection()
        {
            var ename = "LevelDescriptor";
            var entity = this.GetParsed(o => o.XmlSchemaObjectName == ename);
            var sut = new EntityMetadataManager(entity, this.Stub<IXPathMapBuilder>());
            var entityCollectionProperties = sut.GetEntityTypedCollectionProperties();
            entityCollectionProperties.Any(p => p.PropertyName == "LevelDescriptorGradeLevels").ShouldBeTrue();
        }

        [Test]
        public void When_Given_An_Entity_With_A_short_property_type_Should_Correctly_Identify_the_Type()
        {
            var name = "StateEducationAgencyAccountability";
            var propertyName = "SchoolYear";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var sut = new EntityMetadataManager(xsdObject, this.Stub<IXPathMapBuilder>());
            var result = sut.GetSingleSimpleTypedProperties().First(p => p.PropertyName.Equals(propertyName));
            var expectedType = typeof (short);
            result.PropertyType.ShouldEqual(expectedType);
        }

        [Test]
        public void When_Given_An_Entity_With_A_nullable_boolean_property_type_Should_Correctly_Identify_the_Type()
        {
            var name = "StateEducationAgencyAccountability";
            var propertyName = "CTEGraduationRateInclusion";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var sut = new EntityMetadataManager(xsdObject, this.Stub<IXPathMapBuilder>());
            var result = sut.GetSingleSimpleTypedProperties().First(p => p.PropertyName.Equals(propertyName));
            var expectedType = typeof (bool?);
            result.PropertyType.ShouldEqual(expectedType);
        }
 
        [Test]
        public void When_Given_An_Entity_With_A_nullable_int_property_type_Should_Correctly_Identify_the_Type()
        {
            var name = "Location";
            var propertyName = "MaximumNumberOfSeats";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var sut = new EntityMetadataManager(xsdObject, this.Stub<IXPathMapBuilder>());
            var result = sut.GetSingleSimpleTypedProperties().First(p => p.PropertyName.Equals(propertyName));
            var expectedType = typeof (int?);
            result.PropertyType.ShouldEqual(expectedType);
        }
 
        [Test]
        public void When_Given_An_Entity_With_A_nullable_datetime_property_type_Should_Correctly_Identify_the_Type()
        {
            var name = "FeederSchoolAssociation";
            var propertyName = "EndDate";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var sut = new EntityMetadataManager(xsdObject, this.Stub<IXPathMapBuilder>());
            var result = sut.GetSingleSimpleTypedProperties().First(p => p.PropertyName.Equals(propertyName));
            var expectedType = typeof (DateTime?);
            result.PropertyType.ShouldEqual(expectedType);
        }
 
        [Test]
        public void When_Given_An_Entity_With_A_datetime_property_type_Should_Correctly_Identify_the_Type()
        {
            var name = "FeederSchoolAssociation";
            var propertyName = "BeginDate";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var sut = new EntityMetadataManager(xsdObject, this.Stub<IXPathMapBuilder>());
            var result = sut.GetSingleSimpleTypedProperties().First(p => p.PropertyName.Equals(propertyName));
            var expectedType = typeof (DateTime);
            result.PropertyType.ShouldEqual(expectedType);
        }

        [Test]
        public void
            When_Given_An_Entity_With_At_Least_One_InLine_Entity_Collection_And_Asked_For_Them_Should_Return_All()
        {
            var name = "LocalEducationAgency";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var sut = new EntityMetadataManager(xsdObject, this.Stub<IXPathMapBuilder>());
            var result = sut.GetInlineEntityCollectionProperties().Single();
            var propertyAndClassName ="EducationOrganizationCategory";
            result.ClassName.ShouldEqual(propertyAndClassName);
        }

        [Test]
        public void When_Given_Entity_With_At_least_one_Entity_And_Asked_For_Them_Should_Return_All()
        {
            var name = "LearningObjective";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var sut = new EntityMetadataManager(xsdObject, this.Stub<IXPathMapBuilder>());
            var result = sut.GetSingleEntityTypedProperties().Single();
            result.PropertyName.ShouldEqual("LearningObjectiveContentStandard");
            result.ElementName.ShouldEqual("ContentStandard");
        }

        [Test]
        public void When_given_Entity_With_At_Least_one_Entity_Collection_And_Asked_For_Them_Should_return_All()
        {
            var name = "LocalEducationAgency";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var sut = new EntityMetadataManager(xsdObject, this.Stub<IXPathMapBuilder>());
            var result = sut.GetEntityTypedCollectionProperties();
            result.Count().ShouldEqual(5);
            var ability = result.Single(e => e.PropertyName == "LocalEducationAgencyAccountabilities");
            ability.ElementName.ShouldEqual("LocalEducationAgencyAccountability");
        }

        [Test]
        public void
            When_Given_Entity_With_An_Entity_Collection_That_is_Predicated_On_A_Collection_Of_Foreign_Key_Entities_Should_Return_As_Entity_Collection()
        {
            var name = "Course";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var sut = new EntityMetadataManager(xsdObject, this.Stub<IXPathMapBuilder>());
            var result = sut.GetEntityTypedCollectionProperties();
            var expectedEntity = result.Single(e => e.PropertyName == "CourseLearningObjectives");
            var eMM = expectedEntity.GetMetaDataMgr();
            eMM.EntityInterface.ShouldEqual("ICourseLearningObjective");
        }

        [Test]
        public void When_Given_Extended_Reference_Entity_Should_Only_Return_Foreign_Key_Properties()
        {
            var name = "LearningObjectiveReference";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName.Equals(name));
            var stubBuilder = this.Stub<IXPathMapBuilder>();
            stubBuilder.Expect(b => b.BuildStartingXsdElementSerializedMapTuplesFor(Arg<ParsedSchemaObject>.Is.Anything))
                .IgnoreArguments()
                .Return(new List<Tuple<ParsedSchemaObject, string>>
                {
                    new Tuple<ParsedSchemaObject, string>(xsdObject, "Map")
                });
            var sut = new EntityMetadataManager(xsdObject, stubBuilder);
            sut.GetForeignKeyProperties().Any().ShouldBeTrue();
            sut.GetEntityTypedCollectionProperties().Any().ShouldBeFalse();
            sut.GetInlineEntityCollectionProperties().Any().ShouldBeFalse();
            sut.GetSingleEntityTypedProperties().Any().ShouldBeFalse();
            sut.GetSingleSimpleTypedProperties().Any().ShouldBeFalse();
        }

        [Test]
        public void When_Given_Entity_With_At_Least_One_Foreign_Key_Property_And_Asked_For_Them_Should_Return_All()
        {
            //Although the values in the test below do not have to match the schema used the StaffProgramAssociation 
            //element has exactly 4 foreign keys
            var name = "StaffProgramAssociation";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName == name);
            var mapBuilder = this.Stub<IXPathMapBuilder>();
            var tuples = new List<Tuple<ParsedSchemaObject, string>>();
            var genericTuple = new Tuple<ParsedSchemaObject, string>(xsdObject, "path to nowhere");
            tuples.Add(genericTuple);
            tuples.Add(genericTuple);
            tuples.Add(genericTuple);
            tuples.Add(genericTuple);
            mapBuilder.Expect(
                b =>
                    b.BuildStartingXsdElementSerializedMapTuplesFor(
                        Arg<ParsedSchemaObject>.Matches(p => p.Equals(xsdObject)))).IgnoreArguments().Return(tuples);
            var sut = new EntityMetadataManager(xsdObject, mapBuilder);
            var result = sut.GetForeignKeyProperties();
            result.Count().ShouldEqual(mapBuilder.BuildStartingXsdElementSerializedMapTuplesFor(xsdObject).Count());
        }

        [Test]
        [Ignore("Need to check with Q - looks like this is being parsed incorrectly.")]
        public void Should_Load_All_Members_Of_Bell_Schedule()
        {
            var name = "BellSchedule";
            var xsdObject = this.GetParsed(x => x.XmlSchemaObjectName == name);
            var mapBuilder = this.Stub<IXPathMapBuilder>();
            var tuples = new List<Tuple<ParsedSchemaObject, string>>
            {
                new Tuple<ParsedSchemaObject, string>(xsdObject, "who cares")
            };
            mapBuilder.Expect(b => b.BuildStartingXsdElementSerializedMapTuplesFor(Arg<ParsedSchemaObject>.Is.Anything))
                .IgnoreArguments()
                .Return(tuples);
            var sut = new EntityMetadataManager(xsdObject, mapBuilder);
            sut.EntityName.ShouldEqual(name);
            sut.GetEntityTypedCollectionProperties().ShouldBeEmpty();
            sut.GetForeignKeyProperties().ShouldNotBeEmpty();
            sut.GetInlineEntityCollectionProperties().ShouldBeEmpty();
            sut.GetSingleEntityTypedProperties().ShouldBeEmpty();
            sut.GetSingleSimpleTypedProperties().ShouldNotBeEmpty();
        }
    }
}