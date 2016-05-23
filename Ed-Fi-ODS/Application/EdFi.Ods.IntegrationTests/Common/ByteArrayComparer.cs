using System.Linq;

namespace EdFi.Ods.IntegrationTests.Common
{
    public class ByteArrayComparer
    {
        public static bool Compare(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
            {
                return true;
            }
            if ((a1 == null) || (a2 == null)) return false;
            if (a1.Length != a2.Length)
            {
                return false;
            }
            return !a1.Where((t, i) => t != a2[i]).Any();
        }
    }
}
