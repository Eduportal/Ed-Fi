namespace EdFi.Ods.Api.Data.Repositories
{
    public enum TextMatchMode
    {
        Anywhere,
        Start,
        End,
        Exact
    }

    // TODO: Support date/numeric ranges
    //public class DateRangeCriteria : QueryCriteriaBase
    //{
    //    public bool MinInclusive { get; set; }
    //    public bool MaxInclusive { get; set; }

    //    public DateTime MinValue { get; set; }
    //    public DateTime MaxValue { get; set; }
    //}

    //public class NumericRangeCriteria : QueryCriteriaBase
    //{
    //    public bool MinInclusive { get; set; }
    //    public bool MaxInclusive { get; set; }

    //    public decimal MinValue { get; set; }
    //    public decimal MaxValue { get; set; }
    //}
}
