namespace EdFi.Ods.Tests.EdFi.Ods.Admin.Models._Stubs
{
    using global::EdFi.Ods.Admin.Models;

    public class StubUserUpdater : IUpdateUser
    {
        public void UpdateUser(User user)
        {
            this.LastUpdatedUser = user;
        }

        public User LastUpdatedUser { get; private set; }
    }
}