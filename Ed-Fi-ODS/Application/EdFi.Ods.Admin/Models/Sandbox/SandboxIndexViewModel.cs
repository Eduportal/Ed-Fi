namespace EdFi.Ods.Admin.Models.Sandbox
{
    public class SandboxViewModel
    {
        public string User { get; set; }
        public string ApplicationName { get; set; }
        public string Client { get; set; }
        public string Sandbox { get; set; }
    }

    public class SandboxIndexViewModel
    {
        public string[] AllSandboxes { get; set; }
        public SandboxViewModel[] OwnedSandboxes { get; set; }
        public SandboxViewModel[] MissingSandboxes { get; set; }
        public string[] OrphanSandboxes { get; set; }

        public bool HasSandboxes
        {
            get { return AllSandboxes.Length > 0; }
        }

        public bool HasOrphans
        {
            get { return OrphanSandboxes.Length > 0; }
        }

        public bool HasOwnedSandboxes
        {
            get { return OwnedSandboxes.Length > 0; }
        }

        public bool HasMissing
        {
            get { return MissingSandboxes.Length > 0; }
        }
    }
}