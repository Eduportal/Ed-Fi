namespace EdFi.Ods.CodeGen.XmlShredding
{
    public interface IMultiElementEntityCollectionProperty
    {
        string EntityName { get; }
        string PropertyName { get; }
        string ParticipatingElementsAsCommaDelimentedStringOfStrings { get; }
        IManageEntityMetadata GetEntityMetadataManager();
    }
}