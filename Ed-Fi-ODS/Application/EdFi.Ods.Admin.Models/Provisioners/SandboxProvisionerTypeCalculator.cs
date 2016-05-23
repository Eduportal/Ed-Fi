using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Common.Configuration;

namespace EdFi.Ods.Admin.Models
{
    public class SandboxProvisionerTypeCalculator
    {
        public const string AzureConfigValue = "Azure";
        public const string SqlConfigValue = "Sql";
        public const string StubConfigValue = "Stub";
        public const string SandboxTypeKey = "SqlSandboxType";

        private readonly IConfigValueProvider configValueProvider;

        private static readonly Dictionary<string, Type> _provisionerTypes =
            new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase)
                {
                    {AzureConfigValue, typeof (AzureSandboxProvisioner)},
                    {SqlConfigValue, typeof (SqlSandboxProvisioner)},
                    {StubConfigValue, typeof (StubSandboxProvisioner)},
                };

        public SandboxProvisionerTypeCalculator() : this(new AppConfigValueProvider())
        {
        }

        public SandboxProvisionerTypeCalculator(IConfigValueProvider configValueProvider)
        {
            this.configValueProvider = configValueProvider;
        }

        private string GetConfigurationValue()
        {
            return configValueProvider.GetValue(SandboxTypeKey);
        }

        public static Type GetSandboxProvisionerTypeForNewSandboxes()
        {
            return new SandboxProvisionerTypeCalculator().GetSandboxProvisionerType();
        }

        public Type GetSandboxProvisionerType()
        {
            string configurationValue = GetConfigurationValue();
            if (configurationValue == null)
                throw new Exception(string.Format("'{0}' has not been configured. Allowed values are [{1}]", SandboxTypeKey, GetAllowedValues()));

            if (!_provisionerTypes.ContainsKey(configurationValue))
            {
                var message = string.Format("'{0}' is not a valid configuration value for '{1}'. Allowed values are [{2}]",
                                            configurationValue, SandboxTypeKey, GetAllowedValues());
                throw new Exception(message);
            }
            return _provisionerTypes[configurationValue];
        }

        private static string GetAllowedValues()
        {
            return string.Join(", ", _provisionerTypes.Keys.OrderBy(x => x));
        }
    }
}