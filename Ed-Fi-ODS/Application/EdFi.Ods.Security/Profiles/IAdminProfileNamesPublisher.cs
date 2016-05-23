using System.Threading.Tasks;

namespace EdFi.Ods.Security.Profiles
{
    public interface IAdminProfileNamesPublisher
    {
        Task<bool> PublishProfilesAsync();
    }
}
