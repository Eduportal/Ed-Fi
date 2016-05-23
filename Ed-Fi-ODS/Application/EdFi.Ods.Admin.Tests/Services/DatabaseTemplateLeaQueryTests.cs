using EdFi.Ods.Admin.Models;
using EdFi.Ods.Admin.Services;
using NCrunch.Framework;
using NUnit.Framework;
using Should;
using Test.Common;

namespace EdFi.Ods.Admin.Tests.Services
{
    public class DatabaseTemplateLeaQueryTests
    {
        [TestFixture]
        [ExclusivelyUses(TestSingletons.EmptyAdminDatabase)]
        public class When_template_contains_local_education_agencies
        {
            [Test]
            [Ignore("Test data is location specific")]
            public void Should_provide_some_results_for_minimal_template()
            {
                var service = new DatabaseTemplateLeaQuery();
                var results = service.GetAllMinimalTemplateLeaIds();
                var resultsViaType = service.GetLocalEducationAgencyIds(SandboxType.Minimal);

                results.ShouldEqual(resultsViaType);

                //The current minimal template contains 147 rows, but we don't want to force a 
                //tighter test since this actual count would likely change from client to client.
                results.Length.ShouldBeGreaterThan(50);
            }

            [Test]
            public void Should_provide_some_results_for_populated_template()
            {
                var service = new DatabaseTemplateLeaQuery();
                var results = service.GetAllPopulatedTemplateLeaIds();
                var resultsViaType = service.GetLocalEducationAgencyIds(SandboxType.Sample);

                results.ShouldEqual(resultsViaType);

                //The sample data set only contains a single LocalEducationAgency
                results.Length.ShouldBeGreaterThanOrEqualTo(1);
            }
        }
    }
}