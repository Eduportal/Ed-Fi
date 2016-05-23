namespace EdFi.Ods.Tests.EdFi.Ods.Admin.Models._Stubs
{
    using System.Collections.Generic;

    using global::EdFi.Ods.Admin.Models;

    public class TestSandboxProvisioner : ISandboxProvisioner
    {
        private List<Sandbox> _sandboxes = new List<Sandbox>();

        public class Sandbox
        {
            public string Key { get; set; }
            public SandboxType SandboxType { get; set; }
        }

        public Sandbox[] AddedSandboxes
        {
            get { return this._sandboxes.ToArray(); }
        }

        public void AddSandbox(string sandboxKey, SandboxType sandboxType)
        {
            this._sandboxes.Add(new Sandbox{Key = sandboxKey, SandboxType = sandboxType});
        }

        public void DeleteSandboxes(params string[] deletedClientKeys)
        {
            throw new System.NotImplementedException();
        }

        public SandboxStatus GetSandboxStatus(string clientKey)
        {
            throw new System.NotImplementedException();
        }

        public void ResetDemoSandbox()
        {
            throw new System.NotImplementedException();
        }

        public string[] GetSandboxDatabases()
        {
            throw new System.NotImplementedException();
        }
    }
}