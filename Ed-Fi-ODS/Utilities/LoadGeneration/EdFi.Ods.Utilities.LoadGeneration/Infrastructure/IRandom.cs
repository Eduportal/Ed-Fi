namespace EdFi.Ods.Utilities.LoadGeneration.Infrastructure
{
    public interface IRandom
    {
        int Next(int minValueInclusive, int maxValueExclusive);
    }
}