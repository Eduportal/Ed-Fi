namespace EdFi.Ods.Admin.DeployedUsers
{
    public class BuiltinUser : IDeployedUser
    {
        public BuiltinUser()
        {
            Roles = new string[0];
            ClientApis = new TestClientApi[0];
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string[] Roles { get; set; }
        public TestClientApi[] ClientApis { get; set; }
    }
}