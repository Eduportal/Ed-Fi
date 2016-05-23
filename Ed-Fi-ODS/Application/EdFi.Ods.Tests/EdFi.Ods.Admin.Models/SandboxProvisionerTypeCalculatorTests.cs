namespace EdFi.Ods.Tests.EdFi.Ods.Admin.Models
{
    using System;

    using global::EdFi.Common.Configuration;
    using global::EdFi.Ods.Admin.Models;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    public class SandboxProvisionerTypeCalculatorTests
    {
        [TestFixture]
        public class When_azure_is_configured : TestBase
        {
            private Type _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var configurationReader = this.Stub<IConfigValueProvider>();
                configurationReader.Stub(x => x.GetValue(SandboxProvisionerTypeCalculator.SandboxTypeKey))
                                   .Return(SandboxProvisionerTypeCalculator.AzureConfigValue);
                var calculator = new SandboxProvisionerTypeCalculator(configurationReader);

                this._result = calculator.GetSandboxProvisionerType();
            }

            [Test]
            public void Should_indicate_an_azure_provisioner()
            {
                this._result.ShouldEqual(typeof (AzureSandboxProvisioner));
            }
        }

        [TestFixture]
        public class When_sql_is_configured : TestBase
        {
            private Type _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var configurationReader = this.Stub<IConfigValueProvider>();
                configurationReader.Stub(x => x.GetValue(SandboxProvisionerTypeCalculator.SandboxTypeKey))
                                   .Return(SandboxProvisionerTypeCalculator.SqlConfigValue);
                var calculator = new SandboxProvisionerTypeCalculator(configurationReader);

                this._result = calculator.GetSandboxProvisionerType();
            }

            [Test]
            public void Should_indicate_a_sql_provisioner()
            {
                this._result.ShouldEqual(typeof (SqlSandboxProvisioner));
            }
        }

        [TestFixture]
        public class When_stub_is_configured : TestBase
        {
            private Type _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var configurationReader = this.Stub<IConfigValueProvider>();
                configurationReader.Stub(x => x.GetValue(SandboxProvisionerTypeCalculator.SandboxTypeKey))
                                   .Return(SandboxProvisionerTypeCalculator.AzureConfigValue);
                var calculator = new SandboxProvisionerTypeCalculator(configurationReader);

                this._result = calculator.GetSandboxProvisionerType();
            }

            [Test]
            public void Should_indicate_a_stub_provisioner()
            {
                this._result.ShouldEqual(typeof (AzureSandboxProvisioner));
            }
        }

        [TestFixture]
        public class When_some_garbage_value_is_configured : TestBase
        {
            private Exception _thrown;

            [TestFixtureSetUp]
            public void Setup()
            {
                var configurationReader = this.Stub<IConfigValueProvider>();
                configurationReader.Stub(x => x.GetValue(SandboxProvisionerTypeCalculator.SandboxTypeKey))
                                   .Return("Some Garbage Value");
                var calculator = new SandboxProvisionerTypeCalculator(configurationReader);

                try
                {
                    calculator.GetSandboxProvisionerType();
                }
                catch (Exception e)
                {
                    this._thrown = e;
                }
            }

            [Test]
            public void Should_throw_an_exception()
            {
                this._thrown.ShouldNotBeNull();
                const string expectedMessage = "'Some Garbage Value' is not a valid configuration value for 'SqlSandboxType'. Allowed values are [Azure, Sql, Stub]";
                this._thrown.Message.ShouldEqual(expectedMessage);
            }
        }

        [TestFixture]
        public class When_no_value_is_configured : TestBase
        {
            private Exception _thrown;

            [TestFixtureSetUp]
            public void Setup()
            {
                var configurationReader = this.Stub<IConfigValueProvider>();
                configurationReader.Stub(x => x.GetValue(SandboxProvisionerTypeCalculator.SandboxTypeKey))
                                   .Return(null);
                var calculator = new SandboxProvisionerTypeCalculator(configurationReader);

                try
                {
                    calculator.GetSandboxProvisionerType();
                }
                catch (Exception e)
                {
                    this._thrown = e;
                }
            }

            [Test]
            public void Should_throw_an_exception()
            {
                this._thrown.ShouldNotBeNull();
                const string expectedMessage = "'SqlSandboxType' has not been configured. Allowed values are [Azure, Sql, Stub]";
                this._thrown.Message.ShouldEqual(expectedMessage);
            }
        }
    }
}