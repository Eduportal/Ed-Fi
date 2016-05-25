namespace EdFi.Common.StaffIdentity
{
    public interface IUniqueStaffIdentity
    {
        IStaffIdentity Get(string uniqueId);
        IStaffIdentity[] Get(IStaffIdentity identity);
    }
}
