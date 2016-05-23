using EdFi.Ods.Admin.Models;

namespace EdFi.Ods.SecurityConfiguration.Services.Implementation
{
    public interface IAdminContextFactory
    {
        IUsersContext Create();
        void Release(IUsersContext usersContext);
    }
}