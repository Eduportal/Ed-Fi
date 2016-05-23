using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Common.Utils;

namespace EdFi.Ods.Admin.Models
{
    /// <summary>
    /// Test Sandbox Provisioner doesn't actually create sandboxes.
    /// </summary>
    public class StubSandboxProvisioner : ISandboxProvisioner
    {
        static readonly List<string> SandboxKeys = new List<string>();

        public void AddSandbox(string sandboxKey, SandboxType sandboxType)
        {
            if (SandboxKeys.Contains(sandboxKey))
            {
                throw new Exception("At least one sandbox already exists");
            }
            SandboxKeys.Add(sandboxKey);
        }

        public void DeleteSandboxes(params string[] deletedClientKeys)
        {
            if (!deletedClientKeys.ToList().TrueForAll(x => SandboxKeys.Contains(x)))
            {
                throw new Exception("At least one of the sandboxes does not exist");
            }
            SandboxKeys.RemoveAll(deletedClientKeys.Contains);
        }

        public SandboxStatus GetSandboxStatus(string clientKey)
        {
            return SandboxKeys.Contains(clientKey) ? new SandboxStatus() { Code = 0, Description = ApiClientStatus.Online } : SandboxStatus.ErrorStatus();
        }

        public string GetConnectionString(string clientKey)
        {
            throw new Exception("Cannot get a connection to a stubbed sandbox.");
        }

        public void ResetDemoSandbox()
        {
            // Nothing to do 
        }

        public string[] GetSandboxDatabases()
        {
            return SandboxKeys.Select(DatabaseNameBuilder.SandboxNameForKey).ToArray();
        }
    }
}
