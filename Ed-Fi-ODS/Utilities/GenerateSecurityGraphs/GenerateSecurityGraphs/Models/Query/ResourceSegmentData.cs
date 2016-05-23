namespace GenerateSecurityGraphs.Models.Query
{
    public class ResourceSegmentData
    {
        public string ResourceName { get; set; }
        public string ParentResourceName { get; set; }
        public string ActionName { get; set; }
        public string AuthorizationStrategyName { get; set; }
    }
}