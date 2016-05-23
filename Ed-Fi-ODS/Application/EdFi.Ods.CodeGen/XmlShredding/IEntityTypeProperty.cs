namespace EdFi.Ods.CodeGen.XmlShredding
{
    public interface IEntityTypeProperty
    {
        string PropertyName { get; }
        string ElementName { get; }
        IManageEntityMetadata GetMetaDataMgr();
    }
}