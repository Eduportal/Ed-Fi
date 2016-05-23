namespace EdFi.Ods.Tests._Extensions
{
    using System;

    using KellermanSoftware.CompareNetObjects;

    public static class CompareObjectsExtensions
    {
        public static void DumpDifferences(this CompareObjects comparer)
        {
            if (comparer.Differences.Count > 0)
                foreach (var differnce in comparer.Differences)
                {
                    Console.WriteLine("Property {0} difference, expected: {1}, actual: {2}", differnce.PropertyName,
                                      differnce.Object1Value, differnce.Object2Value);
                }
        }
    }
}