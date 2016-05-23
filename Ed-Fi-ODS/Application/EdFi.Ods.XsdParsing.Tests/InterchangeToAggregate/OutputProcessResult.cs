namespace EdFi.Ods.XsdParsing.Tests.InterchangeToAggregate
{
    using EdFi.Ods.CodeGen.XsdToWebApi.Process;

    public class OutputProcessResult
    {
        public static string FormattedResult(ProcessResult processResult)
        {
            var expectedRestType = processResult.Expected as ExpectedRestType;
            if (expectedRestType != null)
                return string.Format("{0}\t{1}\t{2}\t\t", processResult.ProcessingRuleName, FormatExpectedRestClass(expectedRestType), FormatExpectedRestInterface(expectedRestType));

            var expectedRestProperty = processResult.Expected as ExpectedRestProperty;
            if (expectedRestProperty != null)
                return string.Format("{0}\t{1}\t{2}\t{3}\t{4}", processResult.ProcessingRuleName, FormatExpectedRestClass(expectedRestProperty.ContainingExpectedRestType), FormatExpectedRestInterface(expectedRestProperty.ContainingExpectedRestType), FormatExpectedRestPropertyName(expectedRestProperty), FormatExpectedRestPropertyType(expectedRestProperty));

            return string.Format("{0}\t\t\t\t", processResult.ProcessingRuleName);
        }

        private static string FormatExpectedRestType(ExpectedRestType expected)
        {
            if (expected == null)
                return "";
            var typeName = string.Format("{0}.{1}", expected.GetNamespace(), expected.GetClassName());
            if (string.IsNullOrEmpty(expected.GetNamespace()) || expected.GetNamespace() == "System")
                typeName = expected.GetClassName();

            return typeName;
        }
        private static string FormatExpectedRestInterface(ExpectedRestType expected)
        {
            if (expected == null)
                return "";

            if (string.IsNullOrEmpty(expected.GetNamespace()) || expected.GetNamespace() == "System")
                return expected.GetClassName();

            return "I" + expected.GetClassName();
        }

        private static string FormatExpectedRestPropertyType(ExpectedRestProperty expected)
        {
            var propertyTypeName = FormatExpectedRestInterface(expected.PropertyExpectedRestType);
            return expected.IsPropertyCollection ? string.Format("IList<{0}>", propertyTypeName) : propertyTypeName;
            
        }

        private static string FormatExpectedRestPropertyName(ExpectedRestProperty expected)
        {
            var expectedInlineCollection = expected as ExpectedInlineCollection;
            if (expectedInlineCollection == null)
                return expected.PropertyName;

            return string.Format("{0} ({1})", expected.PropertyName, expectedInlineCollection.TerminalRestProperty.PropertyName);
        }

        private static string FormatExpectedRestClass(ExpectedRestType expected)
        {
            if (expected == null)
                return "";

            if (string.IsNullOrEmpty(expected.GetNamespace()) || expected.GetNamespace() == "System")
                return expected.GetClassName();

            return expected.GetNamespace() + "." + expected.GetClassName();
        }
    }
}
