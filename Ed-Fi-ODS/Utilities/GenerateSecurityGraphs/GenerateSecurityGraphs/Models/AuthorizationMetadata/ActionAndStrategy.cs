using System.Collections.Generic;

namespace GenerateSecurityGraphs.Models.AuthorizationMetadata
{
    public class ActionAndStrategy
    {
        public static readonly ActionAndStrategy Empty = new ActionAndStrategy();

        public ActionAndStrategy()
        {
            ExplicitActionAndStrategyByClaimSetName = new Dictionary<string, ActionAndStrategy>();
        }

        public string AuthorizationStrategy { get; set; }
        public string ActionName { get; set; }

        public Dictionary<string, ActionAndStrategy> ExplicitActionAndStrategyByClaimSetName { get; private set; }

        public override string ToString()
        {
            return ActionName 
                + " (" 
                + (AuthorizationStrategy ?? "*No Strategy*") 
                + ")";
        }
    }
}