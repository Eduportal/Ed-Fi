namespace EdFi.Ods.CodeGen.XmlShredding
{
    using System;
    using System.Linq;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using EdFi.Ods.CodeGen.XsdToWebApi.Process;

    public class MultiElementEntityCollectionProperty : IMultiElementEntityCollectionProperty
    {
        private readonly string _elementsAsString;
        private readonly ExpectedRestProperty _target;
        private readonly ParsedSchemaObject _representativeSchemaObject;

        public MultiElementEntityCollectionProperty(ParsedSchemaObject[] participatingSchemaObjects)
        {
            foreach (var schemaObject in participatingSchemaObjects)
            {
                if (!string.IsNullOrWhiteSpace(this._elementsAsString)) this._elementsAsString += ",";
                this._elementsAsString += string.Format(@"""{0}""", schemaObject.XmlSchemaObjectName);
            }
            if(!participatingSchemaObjects.All(p => p.IsRestProperty())) 
                throw new ArgumentException("At least one of these properties is not an Entity Property: {0}", this._elementsAsString);
            this._target = ((ExpectedRestProperty) participatingSchemaObjects[0].ProcessResult.Expected);
            if(participatingSchemaObjects
                .Any(p => ((ExpectedRestProperty)p.ProcessResult.Expected).PropertyExpectedRestType.GetNamespace() != this._target.PropertyExpectedRestType.GetNamespace() ||
                    ((ExpectedRestProperty)p.ProcessResult.Expected).PropertyExpectedRestType.GetClassName() != this._target.PropertyExpectedRestType.GetClassName()))
                throw new ArgumentException(string.Format(
                    @"All objects given to a MultiElementEntityCollectionProperty must have the same Entity Type.  At least one of the supplied elements ({0}) was not type {1}.{2}",
                    this._elementsAsString, this._target.PropertyExpectedRestType.GetNamespace(), this._target.PropertyExpectedRestType.GetClassName()));
            this._representativeSchemaObject = participatingSchemaObjects[0];
        }

        public string EntityName { get { return this._target.PropertyExpectedRestType.GetClassName(); } }
        public string PropertyName { get { return this._target.PropertyName; } }
        public string ParticipatingElementsAsCommaDelimentedStringOfStrings { get { return this._elementsAsString; } }
        public IManageEntityMetadata GetEntityMetadataManager()
        {
            return new EntityMetadataManager(this._representativeSchemaObject, new XPathMapBuilder());
        }
    }
}