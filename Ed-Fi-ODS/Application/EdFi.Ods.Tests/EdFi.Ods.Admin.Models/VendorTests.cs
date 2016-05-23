namespace EdFi.Ods.Tests.EdFi.Ods.Admin.Models
{
    using System.Linq;

    using global::EdFi.Ods.Admin.Models;

    using NUnit.Framework;

    using Should;

    public class VendorTests
    {
        [TestFixture]
        public class When_creating_an_application
        {
            private Vendor _vendor;
            private Application _application;

            [TestFixtureSetUp]
            public void Setup()
            {
                this._vendor = new Vendor {VendorName = "Foo"};
                this._application = this._vendor.CreateApplication("MyApp");
            }

            [Test]
            public void Should_set_application_name()
            {
                this._application.ApplicationName.ShouldEqual("MyApp");
            }

            [Test]
            public void Should_set_vendor()
            {
                this._application.Vendor.ShouldBeSameAs(this._vendor);
            }

            [Test]
            public void Should_add_application_to_vendor_collection()
            {
                var applications = this._vendor.Applications.ToArray();
                applications.Length.ShouldEqual(1);
                applications[0].ShouldBeSameAs(this._application);
            }
        }
    }
}