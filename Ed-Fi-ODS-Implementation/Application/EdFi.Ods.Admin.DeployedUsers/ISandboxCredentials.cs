namespace EdFi.Ods.Admin.DeployedUsers
{
    public interface ISandboxCredentials
    {
        string Key { get; }
        string Secret { get; }
    }
}