using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using EdFi.Ods.Utilities.LoadGeneration._Extensions;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface IResourceCountManager
    {
        void InitializeResourceCount(string resourceName, int count);
        void DecrementCount(string resourceName);
        int TotalRemainingCount { get; }
        decimal TotalProgress { get; }
        IEnumerable<ResourceCount> Resources { get; }

        /// <summary>
        /// Get the progress of the specified resource expressed as a ratio (i.e. between 0.0 and 1.0), 
        /// returning 1.0 if there are no more instances to be generated (even if the original count was 0).
        /// </summary>
        /// <param name="resourceName">The name of the resource.</param>
        /// <returns>The progress represented as a ratio from 0.0 to 1.0.</returns>
        decimal GetProgressForResource(string resourceName);

        /// <summary>
        /// Get the progress of the Student resource expressed as a ratio (i.e. between 0.0 and 1.0), 
        /// returning 1.0 if there are no more instances to be generated (even if the original count was 0).
        /// </summary>
        /// <returns>The progress represented as a ratio from 0.0 to 1.0.</returns>
        decimal GetProgressForStudent();

        /// <summary>
        /// Clear out the resource counts for a new run.
        /// </summary>
        void Reset();
    }

    public class ResourceCountManager : IResourceCountManager
    {
        private ILog _logger = LogManager.GetLogger(typeof(ResourceCountManager));

        private List<ResourceCount> _resourceCounts = new List<ResourceCount>();
        private ConcurrentDictionary<string, ResourceCount> _resourceCountsByName = new ConcurrentDictionary<string, ResourceCount>(StringComparer.InvariantCultureIgnoreCase);

        public void Reset()
        {
            _logger.Debug("Resetting the resource count manager.");
            _resourceCounts = new List<ResourceCount>();
            _resourceCountsByName = new ConcurrentDictionary<string, ResourceCount>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void InitializeResourceCount(string resourceName, int count)
        {
            var resourceCountToAdd = new ResourceCount(resourceName, count);
            bool added = _resourceCountsByName.TryAdd(resourceName, resourceCountToAdd);
            if (added)
            {
                _resourceCounts.Add(resourceCountToAdd);
                return;
            }

            throw new Exception(String.Format("Failed to add {0} to dictionary - does it already exist?", resourceName));
        }

        private readonly HashSet<string> _warnedNoCounts = new HashSet<string>();
        private readonly ReaderWriterLockSlim _warnedNoCountsLock = new ReaderWriterLockSlim();

        public void DecrementCount(string resourceName)
        {
            ResourceCount resourceCount;

            if (_resourceCountsByName.TryGetValue(resourceName, out resourceCount))
            {
                _resourceCountsByName[resourceName].Decrement();
            }
            else
            {
                // TODO: Need unit tests for not blowing up when resource name doesn't exist
                if (_warnedNoCounts.TryAdd(resourceName, _warnedNoCountsLock))
                    _logger.WarnFormat("Resource '{0}' does not have a count registered and cannot be decremented.", resourceName);
            }
        }

        private int GetOriginalCount()
        {
            return Resources.Sum(r => r.OriginalCount);
        }

        private decimal GetTotalProgress()
        {
            if (GetOriginalCount() == 0)
                return 1.0m;

            return 1.0m - ((decimal) TotalRemainingCount/(decimal) GetOriginalCount());
        }

        public int TotalRemainingCount
        {
            get { return Resources.Sum(r => r.Count); }
        }

        public decimal TotalProgress
        {
            get { return GetTotalProgress(); }
        }

        public IEnumerable<ResourceCount> Resources
        {
            get { return _resourceCounts.AsReadOnly(); }
        }

        public decimal GetProgressForResource(string resourceName)
        {
            ResourceCount resource;

            if (!_resourceCountsByName.TryGetValue(resourceName, out resource))
                return 1.0m;

            if (resource.OriginalCount == 0)
                return 1.0m;

            return (decimal) 1.0 - ((decimal)resource.Count / (decimal)resource.OriginalCount);
        }

        public decimal GetProgressForStudent()
        {
            return GetProgressForResource("Student");
        }
    }
}