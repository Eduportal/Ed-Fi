using System;
using System.Collections.Generic;
using EdFi.Ods.Utilities.LoadGeneration.Infrastructure;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface IResourceDataProfileProvider
    {
        IEnumerable<ResourceDataProfile> GetResourceDataProfiles(string fileName);
    }

    public class ResourceDataProfile
    {
        public string ResourceName { get; set; }
        public decimal? PerStudentRatio { get; set; }
        public int? FixedCount { get; set; }
    }

    public class CsvResourceDataProfileProvider : IResourceDataProfileProvider
    {
        private readonly IFile file;

        public CsvResourceDataProfileProvider(IFile file)
        {
            this.file = file;
        }

        public IEnumerable<ResourceDataProfile> GetResourceDataProfiles(string fileName)
        {
            string text = file.ReadAllText(fileName);

            string[] lines = text.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);

            bool headerProcessed = false;
            
            foreach (string line in lines)
            {
                string[] parts = line.Split(new[] {","}, 3, StringSplitOptions.None);

                if (!headerProcessed)
                {
                    // This could actually inspect the first row to make sure it's the header
                    headerProcessed = true;
                    continue;
                }

                // Skip empty entries
                // TODO: Needs unit test
                if (string.IsNullOrWhiteSpace(parts[0]))
                    continue;
                    
                // Skip entries commented out with the '#' symbol
                if (parts[0].StartsWith("#"))
                    continue;

                yield return new ResourceDataProfile()
                {
                    ResourceName = parts[0],
                    PerStudentRatio = string.IsNullOrWhiteSpace(parts[1]) ? null as decimal? : Convert.ToDecimal(parts[1]),
                    FixedCount = string.IsNullOrWhiteSpace(parts[2]) ? null as int? : Convert.ToInt32(parts[2]),
                };
            }

            yield break;
        }
    }
}