using EdFi.Identity.Models;

namespace EdFi.Ods.Tests.EdFi.Identity
{
    using System;

    using global::EdFi.Common.Identity;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class When_Mapping_To_Identity_Resource_Type
    {
        [Test]
        public void Should_Hydrate_All_Fields()
        {
            var identityTest = new global::EdFi.Common.Identity.Identity
                                    {
                                        UniqueId = "123",
                                        BirthDate = new DateTime(2009, 1, 2),
                                        BirthGender = Gender.Female,
                                        FamilyNames = "Smith",
                                        GivenNames = "Bob",
                                        Weight = 11,
                                           IsMatch = true,
                                        Identifiers = new IIdentifier[] { new Identifier() { IdentifierTypeId = 6, Value = "School1" } }
                                    };
            var mapper = new IdentityMapper();
            var mappedResult = mapper.MapToResource(identityTest);
            mappedResult.UniqueId.ShouldEqual(identityTest.UniqueId);
            mappedResult.BirthGender.ShouldEqual(identityTest.BirthGender.ToString());
            mappedResult.BirthDate.ShouldEqual(identityTest.BirthDate);
            mappedResult.GivenNames.ShouldEqual(identityTest.GivenNames);
            mappedResult.FamilyNames.ShouldEqual(identityTest.FamilyNames);
            mappedResult.Weight.ShouldEqual(identityTest.Weight);
                mappedResult.SchoolAssociation.SchoolName.ShouldEqual(identityTest.Identifiers[0].Value);
        }
    }
}