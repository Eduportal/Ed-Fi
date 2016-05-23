using System.Collections.Generic;
using System.Threading;

namespace EdFi.Ods.Utilities.LoadGeneration._Extensions
{
    public static class HashSetExtensions
    {
        public static bool TryAdd<T>(this HashSet<T> hashSet, T item, ReaderWriterLockSlim @lock)
        {
            try
            {
                @lock.EnterUpgradeableReadLock();

                // Reduce log noise by only warning once for each type.
                if (!hashSet.Contains(item))
                {
                    @lock.EnterWriteLock();
                    hashSet.Add(item);
                    @lock.ExitWriteLock();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                @lock.ExitUpgradeableReadLock();
            }
        }
    }
}
