namespace EdFi.Ods.Admin.Models
{
    public class SandboxStatus
    {
        public byte Code { get; set; }
        public string Description { get; set; }

        public static SandboxStatus ErrorStatus()
        {
            return new SandboxStatus()
            {
                Code = byte.MaxValue,
                Description = "ERROR"
            };
        }
    }

    public interface ISandboxProvisioner 
    {
        void AddSandbox(string sandboxKey, SandboxType sandboxType);
        void DeleteSandboxes(params string[] deletedClientKeys);
        SandboxStatus GetSandboxStatus(string clientKey);
        void ResetDemoSandbox();
        string[] GetSandboxDatabases();
    }
}
