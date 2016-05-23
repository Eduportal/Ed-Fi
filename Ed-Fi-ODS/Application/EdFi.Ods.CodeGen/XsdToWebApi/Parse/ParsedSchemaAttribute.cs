namespace EdFi.Ods.CodeGen.XsdToWebApi.Parse
{
    using System.Xml.Schema;

    public class ParsedSchemaAttribute : ParsedSchemaObject
    {
        public override XmlSchemaObject XmlSchemaObject
        {
            get { return this.XmlSchemaAttribute; }
        }

        public override string XmlSchemaObjectName
        {
            get { return this.XmlSchemaAttribute.Name; }
        }

        public XmlSchemaAttribute XmlSchemaAttribute { get; set; }

    }
}