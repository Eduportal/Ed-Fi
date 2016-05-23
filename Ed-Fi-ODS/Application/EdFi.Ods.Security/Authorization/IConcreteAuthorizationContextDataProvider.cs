using EdFi.Ods.Security.AuthorizationStrategies.Relationships;

namespace EdFi.Ods.Security.Authorization
{
    public interface IConcreteEducationOrganizationIdAuthorizationContextDataTransformer<TContextData>
        where TContextData : RelationshipsAuthorizationContextData
    {
        TContextData GetConcreteAuthorizationContextData(TContextData authorizationContextData);
    }
}
