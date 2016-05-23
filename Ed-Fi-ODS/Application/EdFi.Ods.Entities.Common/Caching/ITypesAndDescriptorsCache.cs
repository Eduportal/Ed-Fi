namespace EdFi.Ods.Entities.Common.Caching
{
    public interface ITypesAndDescriptorsCache
    {
        int GetId(string typeName, string value);
        string GetValue(string typeName, int id);
    }
}