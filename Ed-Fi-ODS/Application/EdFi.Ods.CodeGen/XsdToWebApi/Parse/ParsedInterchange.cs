namespace EdFi.Ods.CodeGen.XsdToWebApi.Parse
{
    using System.Xml.Schema;

    public class ParsedInterchange : ParsedSchemaObject
    {
        public override XmlSchemaObject XmlSchemaObject
        {
            get { return this.XmlSchemaElement; }
        }

        public override string XmlSchemaObjectName { get { return this.InterchangeName; } }

        public string InterchangeName { get; set; }

        public XmlSchemaElement XmlSchemaElement { get; set; }
    }
}