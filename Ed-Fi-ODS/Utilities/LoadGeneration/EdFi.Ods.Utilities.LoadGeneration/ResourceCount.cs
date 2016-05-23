using System;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public class ResourceCount
    {
        public ResourceCount(string resourceName, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException();

            Count = count;
            ResourceName = resourceName;
            OriginalCount = count;
        }

        public string ResourceName { get; private set; }
        public int Count { get; private set; }
        public int OriginalCount { get; private set; }
        
        private readonly object _resourceCountLock = new object();

        public void Decrement()
        {
            lock (_resourceCountLock)
            {
                if (Count > 0)
                    Count--;
            }
        }
    }
}  