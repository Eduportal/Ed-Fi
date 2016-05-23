namespace EdFi.Ods.Tests._Bases
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using global::EdFi.Ods.CodeGen.XsdToWebApi;
    using global::EdFi.Ods.CodeGen.XsdToWebApi.Parse;

    using NUnit.Framework;

    public class SchemaMetadataTestBase : TestBase
    {
        private List<ParsedSchemaObject> _objects;

        public SchemaMetadataTestBase()
        {
            this._objects = new List<ParsedSchemaObject>();
            var files = Directory.GetFiles(@"..\..\..\schema.codegen");
            if(!files.Any()) Assert.Fail("No XSD files where found");
            foreach (var file in files)
            {
                this._objects.AddRange(InterchangeLoader.Load(file));
            }
        }

        public ParsedSchemaObject GetParsed(Func<ParsedSchemaObject, bool> findFunction)
        {
            return this._objects.Select(schemaObject => this.GetParsedResursive(schemaObject, findFunction)).FirstOrDefault(result => result != null);
        }

        private ParsedSchemaObject GetParsedResursive(ParsedSchemaObject objectToSearch, Func<ParsedSchemaObject, bool> findFunction)
        {
            if (findFunction(objectToSearch)) return objectToSearch;
            return objectToSearch.ChildElements.Select(schemaObject => this.GetParsedResursive(schemaObject, findFunction)).FirstOrDefault(result => result != null);
        }
    }
}