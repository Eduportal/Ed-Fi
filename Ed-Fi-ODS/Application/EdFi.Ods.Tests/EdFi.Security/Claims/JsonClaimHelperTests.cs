using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using EdFi.Common.Security.Claims;
using EdFi.Ods.Tests._Bases;

using NUnit.Framework;

using Should;

namespace EdFi.Ods.Tests.EdFi.Security.Claims
{
    public class JsonClaimHelperTests
    {
        [TestFixture]
        public class When_serializing_from_EdFiClaimValue_to_Json_claim : TestBase
        {
            const string SuppliedClaimType = "claimType";

            [Test]
            public void Should_have_no_errors_when_suppling_only_the_action()
            {
                var suppliedEdFiClaimValue = new EdFiResourceClaimValue("action");
                var thrown = this.TestForException<Exception>(() => JsonClaimHelper.CreateClaim(SuppliedClaimType, suppliedEdFiClaimValue));
                thrown.ShouldBeNull();
            }

            [Test]
            public void Should_create_claim_when_suppling_only_the_action()
            {
                var suppliedEdFiClaimValue = new EdFiResourceClaimValue("action");
                var actualClaim = JsonClaimHelper.CreateClaim(SuppliedClaimType, suppliedEdFiClaimValue);

                var expectedClaim = new Claim(SuppliedClaimType, "{\"Actions\":[\"action\"]}", "application/json");

                actualClaim.Type.ShouldEqual(expectedClaim.Type);
                actualClaim.ValueType.ShouldEqual(expectedClaim.ValueType);
                actualClaim.Value.ShouldEqual(expectedClaim.Value);
            }

            [Test]
            public void Should_have_no_errors()
            {
                var suppliedEdFiClaimValue = new EdFiResourceClaimValue("action", new List<int>{1,2});
                var thrown = this.TestForException<Exception>(() => JsonClaimHelper.CreateClaim(SuppliedClaimType, suppliedEdFiClaimValue));
                thrown.ShouldBeNull();
            }

            [Test]
            public void Should_create_claim_when_suppling_all_parameters()
            {
                var expected = new Claim(SuppliedClaimType, "{\"Actions\":[\"action\"],\"EducationOrganizationIds\":[1,2]}", "application/json");
                var suppliedEdFiClaimValue = new EdFiResourceClaimValue("action", new List<int> { 1, 2 });
                var actual = JsonClaimHelper.CreateClaim(SuppliedClaimType, suppliedEdFiClaimValue);
                actual.Type.ShouldEqual(expected.Type);
                actual.ValueType.ShouldEqual(expected.ValueType);
                actual.Value.ShouldEqual(expected.Value);
            }
        }

        [TestFixture]
        public class When_deserializing_from_a_Json_claim_to_EdFiClaimValue : TestBase
        {
            const string ExpectedClaimType = "claimType";
            const string ExpectedAction = "action";
            readonly List<int> expectedLocalEducationAgencyIds = new List<int> { 1, 2 };

            [Test]
            public void Should_have_no_errors()
            {
                var suppliedEdFiClaimValue = new EdFiResourceClaimValue(ExpectedAction);
                var claim = JsonClaimHelper.CreateClaim(ExpectedClaimType, suppliedEdFiClaimValue);
                var thrown = this.TestForException<Exception>(() => claim.ToEdFiResourceClaimValue());
                thrown.ShouldBeNull();
            }

            [Test]
            public void Should_create_a_EdFiClaimValue_when_only_action_is_supplied()
            {
                var suppliedEdFiClaimValue = new EdFiResourceClaimValue(ExpectedAction);
                var claim = JsonClaimHelper.CreateClaim(ExpectedClaimType, suppliedEdFiClaimValue);
                var actual = claim.ToEdFiResourceClaimValue();
                actual.Actions.Single().ShouldEqual(ExpectedAction);
                actual.EducationOrganizationIds.ShouldBeEmpty();
            }

            [Test]
            public void Should_create_a_EdFiClaimValue()
            {
                var suppliedEdFiClaimValue = new EdFiResourceClaimValue(ExpectedAction, expectedLocalEducationAgencyIds);
                var claim = JsonClaimHelper.CreateClaim(ExpectedClaimType, suppliedEdFiClaimValue);
                var actual = claim.ToEdFiResourceClaimValue();
                actual.Actions.Single().ShouldEqual(ExpectedAction);
                actual.EducationOrganizationIds.ShouldEqual(expectedLocalEducationAgencyIds);
            }
        }
    }
}
