namespace EdFi.Common.Identity
{
    public interface IIdentifier
    {
        int IdentifierTypeId { get; set; }
        string Value { get; set; }
    }
}