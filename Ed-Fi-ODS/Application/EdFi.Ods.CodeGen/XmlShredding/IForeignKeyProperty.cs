namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System;

    public interface IForeignKeyProperty
    {
        Type PropertyType { get; }
        string PropertyName { get; }
        string ElementName { get; }
        string SerializedReferenceMap { get; }
        bool IsSchoolYear { get; }
    }
}