namespace EdFi.Ods.SecurityConfiguration.Services.Model
{
    public class ActivateResult
    {
        public bool Activated { get; set; }
        public int? LeftChances { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public bool IsValid { get; set; }
    }
}