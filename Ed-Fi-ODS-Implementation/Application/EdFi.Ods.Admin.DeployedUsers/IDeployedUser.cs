namespace EdFi.Ods.Admin.DeployedUsers
{
    public interface IDeployedUser
    {
        string Name { get; set; }
        string Email { get; }
        string Password { get; }
        string[] Roles { get; set; }
        TestClientApi[] ClientApis { get; set; }
    }
}