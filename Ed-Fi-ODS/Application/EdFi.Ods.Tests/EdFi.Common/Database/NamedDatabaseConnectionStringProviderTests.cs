namespace EdFi.Ods.Tests.EdFi.Common.Database
{
    using System;
    using System.Configuration;

    using global::EdFi.Common.Database;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    public class NamedDatabaseConnectionStringProviderTests 
    {
        [TestFixture]
        public class When_calling_the_NamedDatabaseConnectionStringProvider_with_a_valid_name_and_standard_security
        {
            [Test]
            public void Should_Get_Named_Connection_string()
            {
                const string suppliedConnectionStringName = "TestStandardConnectionString";
                var expected = ConfigurationManager.ConnectionStrings[suppliedConnectionStringName].ToString();
                var provider = new NamedDatabaseConnectionStringProvider(suppliedConnectionStringName);
                var actual = provider.GetConnectionString();
                actual.ShouldEqual(expected);
            }
        }

        [TestFixture]
        public class When_calling_the_NamedDatabaseConnectionStringProvider_with_a_valid_name_and_integrated_security
        {
            [Test]
            public void Should_Get_Named_Connection_string()
            {
                const string suppliedConnectionStringName = "TestIntegratedConnectionString";
                var expected = ConfigurationManager.ConnectionStrings[suppliedConnectionStringName].ToString();
                var provider = new NamedDatabaseConnectionStringProvider(suppliedConnectionStringName);
                var actual = provider.GetConnectionString();
                actual.ShouldEqual(expected);
            }
        }

        [TestFixture]
        public class When_calling_the_NamedDatabaseConnectionStringProvider_with_a_NULL_name : TestBase
        {
            private ArgumentNullException _thrown;

            [TestFixtureSetUp]
            public void Setup()
            {
                var provider = new NamedDatabaseConnectionStringProvider(null);
                this._thrown = this.TestForException<ArgumentNullException>(() => provider.GetConnectionString());
            }

            [Test]
            public void Should_throw_an_exception()
            {
                this._thrown.ShouldNotBeNull();
                this._thrown.Message.ShouldContain("connectionStringName");
            }
        }

        [TestFixture]
        public class When_calling_the_NamedDatabaseConnectionStringProvider_with_a_EMPTY_name : TestBase
        {
            private ArgumentNullException _thrown;

            [TestFixtureSetUp]
            public void Setup()
            {
                var provider = new NamedDatabaseConnectionStringProvider(string.Empty);
                this._thrown = this.TestForException<ArgumentNullException>(() => provider.GetConnectionString());
            }

            [Test]
            public void Should_throw_an_exception()
            {
                this._thrown.ShouldNotBeNull();
                this._thrown.Message.ShouldContain("connectionStringName");
            }
        }

        [TestFixture]
        public class When_calling_the_NamedDatabaseConnectionStringProvider_with_a_WHITESPACE_name : TestBase
        {
            private ArgumentNullException _thrown;

            [TestFixtureSetUp]
            public void Setup()
            {
                var provider = new NamedDatabaseConnectionStringProvider(" ");
                this._thrown = this.TestForException<ArgumentNullException>(() => provider.GetConnectionString());
            }

            [Test]
            public void Should_throw_an_exception()
            {
                this._thrown.ShouldNotBeNull();
                this._thrown.Message.ShouldContain("connectionStringName");
            }
        }

    }
}
