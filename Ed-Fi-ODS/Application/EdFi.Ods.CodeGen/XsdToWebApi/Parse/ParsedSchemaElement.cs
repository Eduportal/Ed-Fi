namespace EdFi.Ods.CodeGen.XsdToWebApi.Parse
{
    using System.Xml.Schema;

    public class ParsedSchemaElement : ParsedSchemaObject
    {
        public override XmlSchemaObject XmlSchemaObject
        {
            get { return this.XmlSchemaElement; }
        }

        public override string XmlSchemaObjectName
        {
            get { return this.XmlSchemaElement.Name; }
        }

        public XmlSchemaElement XmlSchemaElement { get; set; }

    }
}