namespace EdFi.Ods.Security.AuthorizationStrategies.NamespaceBased
{
    /// <summary>
    /// Contains the pertinent Ed-Fi data necessary for making authorization decisions.
    /// </summary>
    public class NamespaceBasedAuthorizationContextData
    {
        // Ownership
        public string Namespace { get; set; }
    }
}