namespace EdFi.Common.Identity
{
    public class Identifier : IIdentifier
    {
        public int IdentifierTypeId { get; set; }
        public string Value { get; set; }        
    }
}