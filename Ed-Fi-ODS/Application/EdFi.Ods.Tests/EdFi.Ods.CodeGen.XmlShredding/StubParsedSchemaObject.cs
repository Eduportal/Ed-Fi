namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.XmlShredding
{
    using System.Xml.Schema;

    using global::EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using global::EdFi.Ods.CodeGen.XsdToWebApi.Process;

    public class StubParsedSchemaObject : ParsedSchemaObject
    {
        private readonly string _elementName;

        public StubParsedSchemaObject(string elementName, ProcessResult expected)
        {
            this._elementName = elementName;
            this.ProcessResult = expected;
        }

        public override XmlSchemaObject XmlSchemaObject
        {
            get { throw new System.NotImplementedException(); }
        }

        public override string XmlSchemaObjectName
        {
            get { return this._elementName; }
        }
    }
}