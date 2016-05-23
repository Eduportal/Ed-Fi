namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System;

    public interface IInlineEntityProperty
    {
        string ClassName { get; }
        string InterfaceName { get; }
        string PropertyName { get; }
        string ElementName { get; }
        string InlinePropertyName { get; }
        Type InlinePropertyType { get; }
        string Namespace { get; }
        //TODO: as part of general refactoring this stuff should go away - bad code.  bad.
        bool IsSchoolYear { get; }
        bool IsNested { get; }
        string[] ElementNames { get; }
    }
}