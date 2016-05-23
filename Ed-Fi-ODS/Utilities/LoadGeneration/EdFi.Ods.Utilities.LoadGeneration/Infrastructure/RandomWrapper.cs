using System;

namespace EdFi.Ods.Utilities.LoadGeneration.Infrastructure
{
    public class RandomWrapper : IRandom
    {
        private Random _random = new Random();

        public int Next(int minValueInclusive, int maxValueExclusive)
        {
            return _random.Next(minValueInclusive, maxValueExclusive);
        }
    }
}