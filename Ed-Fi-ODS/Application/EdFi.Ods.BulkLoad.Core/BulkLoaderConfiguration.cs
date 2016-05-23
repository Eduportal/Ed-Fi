using System.Collections.Generic;

namespace EdFi.Ods.BulkLoad.Core
{
    public class BulkLoaderConfiguration
    {
        public string DatabaseNameOverride { get; set; }
        public string SourceFolder { get; set; }
        public string OdsConnectionString { get; set; }
        public Dictionary<string, string>  InterchangeFileTypes { get; set; }

        public bool HasDatabaseName
        {
            get { return !string.IsNullOrEmpty(DatabaseNameOverride); }
        }

        public string Manifest { get; set; }
    }
}