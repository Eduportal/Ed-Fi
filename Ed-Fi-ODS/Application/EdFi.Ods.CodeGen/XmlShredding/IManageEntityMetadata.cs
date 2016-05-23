namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System.Collections.Generic;

    public interface IManageEntityMetadata
    {
        string EntityName { get; }
        string ResourceNamespace { get; }
        string EntityInterface { get; }
        IEnumerable<ISimpleTypeProperty> GetSingleSimpleTypedProperties();                               //string, int...
        IEnumerable<IInlineEntityProperty> GetInlineEntityCollectionProperties();                        //collection of other simple entities (typically descriptors)
        IEnumerable<IEntityTypeProperty> GetSingleEntityTypedProperties();                               //single other entity in a 1:1 relationship (example School.District)
        IEnumerable<IEntityTypeProperty> GetEntityTypedCollectionProperties();                           //1:* other entities (District.Schools)
        IEnumerable<IForeignKeyProperty> GetForeignKeyProperties();                                      //Look like simple type properties, but cannot be parsed from the XML that way. Typically a refernce or a complex XML element to get to the simple value
        IEnumerable<IMultiElementEntityCollectionProperty> GetMultiElementEntityCollectionProperties();  //Something like a descriptor that has multiple simple values (StudentIdentificationDocuments)
        bool IsDescriptorsExtRef();
    }
}