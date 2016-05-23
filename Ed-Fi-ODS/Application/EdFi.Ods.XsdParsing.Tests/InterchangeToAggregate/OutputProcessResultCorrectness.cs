namespace EdFi.Ods.XsdParsing.Tests.InterchangeToAggregate
{
    using EdFi.Ods.CodeGen.XsdToWebApi.Process;

    public class OutputProcessResultCorrectness
    {
        
        public static string FormattedResult(ProcessResult processResult)
        {
            var expectedRestType = processResult.Expected as ExpectedRestType;
            if (expectedRestType != null)
                return string.Format("{0}\t\t\t{1}", Validate.IsRestInterfaceCorrect(expectedRestType), Validate.IsRestClassCorrect(expectedRestType));

            var expectedRestProperty = processResult.Expected as ExpectedRestProperty;
            if (expectedRestProperty != null)
                return string.Format("{0}\t{1}\t{2}\t{3}", Validate.IsRestInterfaceCorrect(expectedRestProperty.ContainingExpectedRestType), Validate.IsRestPropertyCorrect(expectedRestProperty), Validate.IsRestPropertyTypeCorrect(expectedRestProperty), Validate.IsRestClassCorrect(expectedRestProperty.ContainingExpectedRestType));

            return "\t\t\t";
        }
    }
}