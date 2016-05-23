using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Parse
{
    class ParsedSchemaComplexType: ParsedSchemaObject
    {
        public override XmlSchemaObject XmlSchemaObject
        {
            get { return this.XmlComplexSchemaObject; }
        }

        public override string XmlSchemaObjectName { get { return this.XmlComplexSchemaObject.Name; } }
        
        public XmlSchemaComplexType XmlComplexSchemaObject { get; set; }
    }

}
