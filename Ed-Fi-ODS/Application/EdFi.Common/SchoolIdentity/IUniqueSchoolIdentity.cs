namespace EdFi.Common.SchoolIdentity
{
    public interface IUniqueSchoolIdentity
    {
        ISchoolIdentity Get(string uniqueId);
        ISchoolIdentity[] Get(ISchoolIdentity identity);
    }
}
