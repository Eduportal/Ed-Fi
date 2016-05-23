using System.Collections.Generic;

namespace EdFi.Ods.BulkLoad.Core
{
    public class LoadResult
    {
        public LoadResult()
        {
            LoadExceptions = new List<LoadException>();
        }

        public List<LoadException> LoadExceptions { get; set; }
        public int LoadedResourceCount { get; set; }
        public int SourceElementCount { get; set; }
    }
}