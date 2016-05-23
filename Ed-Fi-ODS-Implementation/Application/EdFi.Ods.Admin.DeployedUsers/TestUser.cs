namespace EdFi.Ods.Admin.DeployedUsers
{
    public class TestUser : IDeployedUser
    {
        public TestUser()
        {
            Roles = new string[0];
            ClientApis = new TestClientApi[0];
        }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password
        {
            get { return Name; }
        }

        public string[] Roles { get; set; }
        public TestClientApi[] ClientApis { get; set; }
    }
}