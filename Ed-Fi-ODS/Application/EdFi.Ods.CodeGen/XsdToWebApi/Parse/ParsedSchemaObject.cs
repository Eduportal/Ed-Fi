using System.Collections.Generic;
using System.Xml.Schema;
using EdFi.Ods.CodeGen.XsdToWebApi.Extensions;
using EdFi.Ods.CodeGen.XsdToWebApi.Process;

namespace EdFi.Ods.CodeGen.XsdToWebApi.Parse
{
    public abstract class ParsedSchemaObject
    {
        protected ParsedSchemaObject()
        {
            this.ChildElements = new List<ParsedSchemaObject>();
        }

        public abstract XmlSchemaObject XmlSchemaObject { get; }
        public abstract string XmlSchemaObjectName { get; }
        public XmlSchemaType XmlSchemaType { get; set; }
        public XmlSchemaType ParentXmlSchemaType { get; set; }
        public XmlSchemaType DeclaringSchemaType { get; set; }
        public string XmlSchemaTypeGroup { get; set; }
        public bool IsComplexType { get; set; }
        public bool IsOptional { get; set; }
        public bool IsCollection { get; set; }
        public bool IsAttribute { get; set; }
        public ISchemaValidation[] SchemaValidations { get; set; }
        public List<ParsedSchemaObject> ChildElements { get; set; }
        public ParsedSchemaObject ParentElement { get; set; }
        public string Documentation { get; set; }
        public ProcessResult ProcessResult { get; set; }
        public bool IsExtension { get; set; }

        public string GetParentTypeName()
        {
            var parentTypeName = this.DeclaringSchemaType.Name.StripExtensionNameValues();
            if (this.ParentElement.IsCommonSchemaTypeGroup() && CommonExpansion.IsCommonExpansionSchemaType(parentTypeName))
                parentTypeName = this.ParentElement.DeclaringSchemaType.Name.StripExtensionNameValues();
            return parentTypeName;
        }
        
        public bool IsCommonSchemaTypeGroup()
        {
            return this.XmlSchemaTypeGroup == "Common";
        }
    }
}