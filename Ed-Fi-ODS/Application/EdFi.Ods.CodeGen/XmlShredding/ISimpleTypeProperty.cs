namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System;

    public interface ISimpleTypeProperty
    {
        Type PropertyType { get; }
        string ElementName { get; }
        string PropertyName { get; }
        //TODO: these need to go . . . somewhere
        bool IsSchoolYear { get; }
    }
}