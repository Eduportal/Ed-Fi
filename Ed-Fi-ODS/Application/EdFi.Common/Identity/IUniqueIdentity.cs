namespace EdFi.Common.Identity
{
    public interface IUniqueIdentity
    {
        IIdentity Get(string uniqueId);
        IIdentity[] Get(IIdentity identity);
        IIdentity Post(IIdentity command);
    }
}
