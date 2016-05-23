namespace EdFi.Ods.XsdParsing.Tests
{
    using System.Collections.Generic;

    using EdFi.Ods.CodeGen.XsdToWebApi.Parse;
    using EdFi.Ods.CodeGen.XsdToWebApi.Process;

    public static class ParsedSchemaObjectExtension
    {
        public static IEnumerable<ParsedSchemaObject> AllElements(this ParsedSchemaObject parsedSchemaObject)
        {
            if (parsedSchemaObject.ProcessResult == null)
                yield break;

            yield return parsedSchemaObject;

            foreach (var child in parsedSchemaObject.ChildElements)
            {
                foreach (var grandchild in child.AllElements())
                {
                    yield return grandchild;
                }
            }
        }
        
        public static IEnumerable<ParsedSchemaObject> AllTerminalElements(this ParsedSchemaObject parsedSchemaObject)
        {
            if (parsedSchemaObject.ProcessResult == null)
                yield break;

            if (parsedSchemaObject.ProcessResult.Expected is ExpectedTerminalRestProperty)
                yield return parsedSchemaObject;

            if (parsedSchemaObject.ProcessResult is ReferenceTypeProcessResult)
                yield break;

            foreach (var child in parsedSchemaObject.ChildElements)
            {
                foreach (var grandchild in child.AllTerminalElements())
                {
                    yield return grandchild;
                }
            }
        }

        public static IEnumerable<ParsedSchemaObject> AllReferenceElements(this ParsedSchemaObject parsedSchemaObject)
        {
            if (parsedSchemaObject.ProcessResult == null)
                yield break;

            if (parsedSchemaObject.ProcessResult is ReferenceTypeProcessResult)
                yield return parsedSchemaObject;

            foreach (var child in parsedSchemaObject.ChildElements)
            {
                foreach (var grandchild in child.AllReferenceElements())
                {
                    yield return grandchild;
                }
            }
        }
    }
}
