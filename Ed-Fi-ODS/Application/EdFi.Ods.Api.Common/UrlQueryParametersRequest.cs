namespace EdFi.Ods.Api.Common
{
    public class UrlQueryParametersRequest 
    {
        public int? Offset { get; set; }
        public int? Limit { get; set; }
        public string Q { get; set; }
    }
}
