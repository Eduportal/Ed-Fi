using EdFi.Ods.Admin.Models;

namespace EdFi.Ods.Admin.DeployedUsers
{
    public class TestClientApi : ISandboxCredentials
    {
        public TestClientApi()
        {
            EducationOrganizationIds = new int[0];
        }

        public string Name { get; set; }
        public string Key { get; set; }
        public string Secret { get; set; }
        public SandboxType SandboxType { get; set; }
        public bool FakeSandbox { get; set; }
        public int[] EducationOrganizationIds { get; set; }
    }
}