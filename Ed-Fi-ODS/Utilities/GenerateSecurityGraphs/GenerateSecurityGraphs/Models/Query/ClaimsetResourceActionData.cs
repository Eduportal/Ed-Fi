namespace GenerateSecurityGraphs.Models.Query
{
    internal class ClaimsetResourceActionData
    {
        public string ClaimSetName { get; set; }
        public string ResourceName { get; set; }
        public string ActionName { get; set; }
        public string StrategyName { get; set; } // For future use.

        // This property implementation used for interactive testing purposes
        //public string StrategyName
        //{
        //    get
        //    {
        //        if (ClaimSetName == "SIS Vendor"
        //            && ResourceName == "course"
        //            && ActionName == "Read")
        //        {
        //            return "PrimaryRelationships";
        //            //return "NoFurtherAuthorizationRequired";
        //        }

        //        return null;
        //    }
        //    set
        //    {
        //        // Do nothing
        //    }
        //}
    }
}