using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace EdFi.Ods.Utilities.LoadGeneration.Persistence
{
    /// <summary>
    /// Provides methods for maintaining a known list of associations of education organizations to people.
    /// </summary>
    public interface IPersonEducationOrganizationCache
    {
        /// <summary>
        /// Associates a unique id to an education organization.
        /// </summary>
        /// <param name="personType">One of the support person types (e.g. Staff, Student or Parent).</param>
        /// <param name="uniqueId">The person's UniqueId value.</param>
        /// <param name="educationOrganizationId">The identifier of the education organization.</param>
        /// <returns>An instance of <see cref="ICommittable"/> which enables the value added to be seen by other threads.</returns>
        ICommittable AddPersonEducationOrganizationAssociation(string personType, string uniqueId, int educationOrganizationId);
        
        void RemovePersonEducationOrganizationAssociation(string personType, string uniqueId, int educationOrganizationId);

        /// <summary>
        /// Gets the person UniqueIds associated with the specified education organization.
        /// </summary>
        /// <param name="personType">One of the support person types (e.g. Staff, Student or Parent).</param>
        /// <param name="educationOrganizationId">The identifier of the education organization.</param>
        /// <returns>A list of associated UniqueIds.</returns>
        IList<string> GetPeople(string personType, int educationOrganizationId);
    }

    public interface ICommittable
    {
        void Commit();
    }

    // TODO: Needs unit tests
    public class PersonEducationOrganizationCache : IPersonEducationOrganizationCache
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, List<UniqueId>>> _uniqueIdsByEdOrgIdByPersonType
            = new ConcurrentDictionary<string, ConcurrentDictionary<int, List<UniqueId>>>(StringComparer.InvariantCultureIgnoreCase);

        private class UniqueId : ICommittable
        {
            public UniqueId(string uniqueId)
            {
                _lock = new ReaderWriterLockSlim();
                _lock.EnterWriteLock();
                _value = uniqueId;
            }

            private ReaderWriterLockSlim _lock;

            private readonly string _value;
            private bool _isCommitted;

            public string GetValue()
            {
                return GetValue(TimeSpan.Zero);
            }

            public string GetValue(TimeSpan timeout)
            {
                // Allow the original creator access to the value during initialization
                if (_isCommitted || _lock.IsWriteLockHeld)
                    return _value;
                
                // Wait for the writer to commit the value
                if (_lock.TryEnterReadLock(timeout))
                {
                    try
                    {
                        return _value;
                    }
                    finally
                    {
                        _lock.ExitReadLock();

                        // Drop lock altogether once it is not being used anymore
                        if (_lock.CurrentReadCount == 0)
                        {
                            _lock.Dispose();
                            _lock = null;
                        }
                    }       
                }

                return null;
                //throw new Exception(string.Format("Unable to gain read access to UniqueId value '{0}' within 10 seconds.", _value));
            }

            public void Commit()
            {
                if (_lock.IsWriteLockHeld)
                {
                    _isCommitted = true;
                    _lock.ExitWriteLock();

                    return;
                }

                throw new InvalidOperationException(string.Format("A thread called Commit without the write lock to UniqueId '{0}'.", _value));
            }
        }

        public ICommittable AddPersonEducationOrganizationAssociation(string personType, string uniqueId, int educationOrganizationId)
        {
            var uniqueIdsByEdOrgId = _uniqueIdsByEdOrgIdByPersonType.GetOrAdd(personType, key => new ConcurrentDictionary<int, List<UniqueId>>());

            var uniqueIds = uniqueIdsByEdOrgId.GetOrAdd(educationOrganizationId, key => new List<UniqueId>());

            lock (uniqueIds)
            {
                var item = new UniqueId(uniqueId);
                uniqueIds.Add(item);
                return item;
            }
        }

        public void RemovePersonEducationOrganizationAssociation(string personType, string uniqueId, int educationOrganizationId)
        {
            var uniqueIdsByEdOrgId = _uniqueIdsByEdOrgIdByPersonType.GetOrAdd(personType, key => new ConcurrentDictionary<int, List<UniqueId>>());

            var uniqueIds = uniqueIdsByEdOrgId.GetOrAdd(educationOrganizationId, key => new List<UniqueId>());

            lock (uniqueIds)
            {
                var uniqueIdEntry = uniqueIds.SingleOrDefault(x => x.GetValue() == uniqueId);

                if (uniqueIdEntry != null)
                    uniqueIds.Remove(uniqueIdEntry);
            }
        }

        public IList<string> GetPeople(string personType, int educationOrganizationId)
        {
            ConcurrentDictionary<int, List<UniqueId>> uniqueIdsByEdOrgId;

            if (!_uniqueIdsByEdOrgIdByPersonType.TryGetValue(personType, out uniqueIdsByEdOrgId))
                return new List<string>();

            List<UniqueId> uniqueIds;

            if (uniqueIdsByEdOrgId.TryGetValue(educationOrganizationId, out uniqueIds))
            {
                // Filter out values that are not ready for consumption (i.e. unique Ids for which the necessary steps for authorization haven't yet completed)
                return uniqueIds.Select(x => x.GetValue()).Where(x => x != null).ToList();
            }

            return new List<string>();
        }
    }
}