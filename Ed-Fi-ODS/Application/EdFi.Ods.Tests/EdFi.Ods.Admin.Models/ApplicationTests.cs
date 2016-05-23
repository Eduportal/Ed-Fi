namespace EdFi.Ods.Tests.EdFi.Ods.Admin.Models
{
    using System.Linq;

    using global::EdFi.Ods.Admin.Models;

    using NUnit.Framework;

    using Should;

    public class ApplicationTests
    {
        [TestFixture]
        public class When_creating_an_application_local_education_agency
        {
            private Application _application;
            private ApplicationEducationOrganization _lea;

            [TestFixtureSetUp]
            public void Setup()
            {
                this._application = new Application{ApplicationName = "Foo"};
                this._lea = this._application.CreateEducationOrganizationAssociation(42);
            }

            [Test]
            public void Should_set_lea_id()
            {
                this._lea.EducationOrganizationId.ShouldEqual(42);
            }

            [Test]
            public void Should_set_application()
            {
                this._lea.Application.ShouldBeSameAs(this._application);
            }

            [Test]
            public void Should_add_association_to_application_collection()
            {
                var leas = this._application.ApplicationEducationOrganizations.ToArray();
                leas.Length.ShouldEqual(1);
                leas[0].ShouldBeSameAs(this._lea);
            }
        }
    }
}