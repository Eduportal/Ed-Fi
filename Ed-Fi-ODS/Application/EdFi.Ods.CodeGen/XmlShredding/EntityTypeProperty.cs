namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using EdFi.Ods.CodeGen.XsdToWebApi.Process;

    public class EntityTypeProperty : IEntityTypeProperty
    {
        private ParsedSchemaObject _property;

        public EntityTypeProperty(ParsedSchemaObject property)
        {
            if (!property.IsEntityProperty())
                    throw new ArgumentException(string.Format("{0} is a {1} and not an Entity Type (ExpectedRestProperty or ExpectedInlineCollection)", property.XmlSchemaObjectName, property.ProcessResult.Expected.GetType()));
            this._property = property;
        }

        public string PropertyName { get { return ((ExpectedRestProperty)this._property.ProcessResult.Expected).PropertyName; } }

        public string ElementName { get { return this._property.XmlSchemaObjectName; } }

        public IManageEntityMetadata GetMetaDataMgr()
        {
            return new EntityMetadataManager(this._property, new XPathMapBuilder());
        }
    }
}