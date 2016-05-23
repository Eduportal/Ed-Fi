using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface IExistingResourceReferenceProvider
    {
        void AddResourceReference(object resourceReference);

        /// <summary>
        /// Gets a attempts to locate an existing resource reference given a set of value constraints.
        /// </summary>
        /// <param name="resourceReferenceType">The Type of the reference to retrieve.</param>
        /// <param name="propertyValueConstraints">A dictionary of property values to be used to constrain the selection, or <b>null</b>.</param>
        /// <returns>A randomly selected reference that satisifies the supplied constraints; otherwise <b>null</b>.</returns>
        object GetResourceReference(Type resourceReferenceType, IDictionary<string, object> propertyValueConstraints);
    }

    public class ExistingResourceReferenceProvider : IExistingResourceReferenceProvider
    {
        private readonly IPropertyConstraintsCollectionFilter _collectionFilter;

        public ExistingResourceReferenceProvider(IPropertyConstraintsCollectionFilter collectionFilter)
        {
            _collectionFilter = collectionFilter;
        }

        public class ResourceReferenceCollection
        {
            //private readonly object locker = new object();
            private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
            private static readonly Random random = new Random();
            private readonly Type resourceType;
            private readonly IPropertyConstraintsCollectionFilter _collectionFilter;
            private List<object> referenceResources;


            // TODO: Need unit tests for this behavior (of limiting resource references held in memory based on actual usage)
            // This value controls the maximum number of references that will be held if they are never requested
            private const int UnusedReferenceTypeCapacity = 500;
            private bool _referencesRequested = false;

            public ResourceReferenceCollection(Type resourceType, IPropertyConstraintsCollectionFilter collectionFilter)
            {
                this.resourceType = resourceType;
                _collectionFilter = collectionFilter;
                referenceResources = new List<object>();
            }

            public void Add(object resourceReference)
            {
                if (resourceReference.GetType() != resourceType)
                    throw new ArgumentException(string.Format("Unable to add resource of type '{0}' to a collection of type '{1}'.",resourceReference.GetType().AssemblyQualifiedName, resourceType.AssemblyQualifiedName));

                // If this reference type isn't being used, then stop saving them in memory
                if (!_referencesRequested && Count >= UnusedReferenceTypeCapacity)
                    return;

                locker.EnterWriteLock();

                try
                {
                    referenceResources.Add(resourceReference);
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }

            public object GetRandomMember(IDictionary<string, object> propertyValueConstraints)
            {
                locker.EnterReadLock();

                try
                {
                    _referencesRequested = true;

                    var resourcesThatMeetConstraints =
                        _collectionFilter.Filter(referenceResources, propertyValueConstraints).ToList();

                    if (!resourcesThatMeetConstraints.Any())
                        return null;

                    var randomIndex = random.Next(resourcesThatMeetConstraints.Count());

                    return resourcesThatMeetConstraints.ElementAt(randomIndex);
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }

            public object GetRandomMember()
            {
                locker.EnterReadLock();

                try
                {
                    _referencesRequested = true;

                    if (!referenceResources.Any())
                        return null;

                    var randomIndex = random.Next(referenceResources.Count());
                    return referenceResources.ElementAt(randomIndex);
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }

            // TODO: Need unit tests for count
            public int Count
            {
                get
                {
                    locker.EnterReadLock();

                    try
                    {
                        return referenceResources.Count;
                    }
                    finally
                    {
                        locker.ExitReadLock();    
                    }
                }
            }
        }

        private readonly ConcurrentDictionary<Type, ResourceReferenceCollection> _resourceReferenceCollectionByReferenceType
            = new ConcurrentDictionary<Type, ResourceReferenceCollection>();

        public void AddResourceReference(object resourceReference)
        {
            var rm = _resourceReferenceCollectionByReferenceType.GetOrAdd(resourceReference.GetType(), type => new ResourceReferenceCollection(type, _collectionFilter));

            rm.Add(resourceReference);
        }

        public object GetResourceReference(Type resourceReferenceType, IDictionary<string, object> propertyValueConstraints)
        {
            // TODO: Needs unit test around ensuring case-insensitive matching is performed
            // Make sure that the constraints provided are case-insensitive
            if (propertyValueConstraints == null)
                propertyValueConstraints = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            var constraintsAsDictionary = propertyValueConstraints as Dictionary<string, object>;

            if (constraintsAsDictionary != null)
                if (constraintsAsDictionary.Comparer.GetType() != StringComparer.InvariantCultureIgnoreCase.GetType())
                    throw new ArgumentException("Dictionary provided must use a case-insensitive key comparer.", "resourceReferenceType");

            var referenceCollection = GetResourceReferenceCollection(resourceReferenceType);
            
            // TODO: Need to add support for not returning references when filter results in less than 2 results as this can cause PK violations in child lists
            return referenceCollection == null ? null : referenceCollection.GetRandomMember(propertyValueConstraints);
        }

        private ResourceReferenceCollection GetResourceReferenceCollection(Type resourceReferenceType)
        {
            ResourceReferenceCollection resourceReferenceCollection;

            if (_resourceReferenceCollectionByReferenceType.TryGetValue(resourceReferenceType, out resourceReferenceCollection))
                return resourceReferenceCollection;
            
            return null;
        }
    }
}
