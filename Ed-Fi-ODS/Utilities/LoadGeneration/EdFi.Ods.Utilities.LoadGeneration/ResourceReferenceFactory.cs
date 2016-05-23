using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Castle.MicroKernel.Registration;
using EdFi.Ods.Utilities.LoadGeneration._Extensions;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface IResourceReferenceFactory
    {
        object CreateResourceReference(object resourceInstance);
    }
    
    public class ResourceReferenceFactory : IResourceReferenceFactory
    {
        private ILog _logger = LogManager.GetLogger(typeof(ResourceReferenceFactory));

        public ResourceReferenceFactory(IApiSdkReflectionProvider apiSdkReflectionProvider)
        {
            _apiSdkReflectionProvider = apiSdkReflectionProvider;
        }
        private readonly IApiSdkReflectionProvider _apiSdkReflectionProvider;
        
        private readonly HashSet<Type> _warnedTypes = new HashSet<Type>();
        private readonly ReaderWriterLockSlim _warnedTypesLock = new ReaderWriterLockSlim();

        public object CreateResourceReference(object resourceInstance)
        {
            var resourceType = resourceInstance.GetType();
            
            Type referenceType;

            if (!_apiSdkReflectionProvider.TryGetReferenceType(resourceType, out referenceType))
            {
                // TODO: Need unit test for case of reference type not existing

                // Only warn once
                if (_warnedTypes.TryAdd(resourceType, _warnedTypesLock))
                    _logger.WarnFormat("Unable to create a reference for '{0}' because no reference type could be found.", resourceType.Name);
                
                return null;
            }

            // Create the reference
            var reference = Activator.CreateInstance(referenceType);

            var referenceProperties = referenceType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            var sourceValueCandidatesByPropertyName = referenceProperties.ToDictionary(x => x.Name, x => new List<CandidateSourceProperty>(), StringComparer.InvariantCultureIgnoreCase);
           
            GatherValuesFromObject(resourceInstance, sourceValueCandidatesByPropertyName, null, 1);

            int previousCount = int.MaxValue;

            while (previousCount != 0)
            {
                previousCount = referenceProperties.Count;

                // Iterate through its properties, setting property values from the values gathered
                foreach (var referenceProperty in referenceProperties.ToList())
                {
                    var candidateSources = sourceValueCandidatesByPropertyName[referenceProperty.Name];
                    candidateSources.Sort();

                    var topCandidates =
                        (from c in candidateSources
                         orderby c.Depth
                         group c by c.Score into g
                         orderby g.Key descending
                         select g)
                         .FirstOrDefault();

                    if (topCandidates == null)
                    {
                        referenceProperties.Remove(referenceProperty);
                        continue;

                        //throw new Exception(
                        //    string.Format("Unable to locate candidates for property '{0}' on reference type '{1}'.",
                        //        referenceProperty.Name, referenceType.Name));
                    }

                    if (topCandidates.Count() == 1 // there's a clear "winner"
                        || topCandidates.Select(x => x.Value).Distinct().Count() == 1 // they all agree on the same value
                        || topCandidates.ElementAt(0).Depth == 1 && topCandidates.ElementAt(1).Depth > 1) 
                            // prioritize properties on the main object over its references (e.g. Assessment.title vs. Assessment.assessmentFamilyReference.title or Assessment.contentStandard.title)
                    {
                        // We found an obvious winner
                        referenceProperty.SetValue(reference, topCandidates.First().Value);

                        var sourceProperty = topCandidates.First().Property;

                        // Remove the source property from contention on all other properties
                        foreach (var kvp in sourceValueCandidatesByPropertyName)
                        {
                            // Find any occurrence of this source property
                            var candidatesToRemove =
                                (from c in kvp.Value
                                    where c.Property == sourceProperty
                                    select c)
                                    .ToList();

                            // Remove the source property from contention on other properties
                            foreach (var candidateSourceProperty in candidatesToRemove)
                                kvp.Value.Remove(candidateSourceProperty);
                        }

                        referenceProperties.Remove(referenceProperty);
                    }
                    else
                    {
                        _logger.DebugFormat("Candidates for property '{0}' on reference type '{1}': {2}",
                            referenceProperty.Name, referenceType.Name, string.Join(", ", topCandidates.Select(x => x.Property.DeclaringType.Name + "." + x.Property.Name)));
                    }
                }

                if (referenceProperties.Count == previousCount)
                    throw new Exception(string.Format("Unable to clearly identify the source and target properties for building reference type '{0}'.", referenceType.Name));

                previousCount = referenceProperties.Count;
            }

            // Return the newly created reference with all properties that could be found
            return reference; 
        }

        private class CandidateSourceProperty : IComparable<CandidateSourceProperty>
        {
            public PropertyInfo Property { get; set; }
            public int Score { get; set; }
            public object Value { get; set; }
            public int Depth { get; set; }

            public int CompareTo(CandidateSourceProperty other)
            {
                return Score.CompareTo(other.Score);
            }
        }

        private void GatherValuesFromObject(object source, IDictionary<string, List<CandidateSourceProperty>> sourceCandidatesByPropertyName, string roleName, int depth)
        {
            var sourceProperties = 
                (from p in source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where !p.Name.Equals("id", StringComparison.InvariantCultureIgnoreCase) // Don't want id's getting pulled in by class name as property prefix convention
                select p)
                .ToList();
            
            string referencedTypeName = Regex.Replace(source.GetType().Name, @"^([\w\d]+)Reference$", "$1");

            // For every Property in the type
            foreach (var sourceProperty in sourceProperties.OrderBy(PrioritizeScalarPropertiesOverReferences()))
            {
                // If the property is a non-.NET Framework object reference
                if (sourceProperty.PropertyType.IsCustomClass())
                {
                    var objectReference = sourceProperty.GetValue(source);

                    if (objectReference != null)
                    {
                        // TODO: Need unit test for passing role name through
                        string referenceRoleName = GetRoleName(sourceProperty.Name, sourceProperty.PropertyType.Name);
                        GatherValuesFromObject(objectReference, sourceCandidatesByPropertyName, referenceRoleName, depth + 1);
                    }

                    continue;
                }

                // If the property is one we're looking for, and the value has not yet been assigned, capture it.
                ProcessSourceCandidate(sourceCandidatesByPropertyName, sourceProperty.Name, source, sourceProperty, 1, depth);

                // Try using the role name to make the match, and assign it if not already assigned
                ProcessSourceCandidate(sourceCandidatesByPropertyName, roleName + sourceProperty.Name, source, sourceProperty, 2, depth);

                // Try using the convention where class name as prefix is trimmed from property name
                ProcessSourceCandidate(sourceCandidatesByPropertyName, referencedTypeName + sourceProperty.Name, source, sourceProperty, 2, depth);
            }
        }

        private static Func<PropertyInfo, int> PrioritizeScalarPropertiesOverReferences()
        {
            return p => p.PropertyType.IsCustomClass() ? 2 : 1;
        }

        private void ProcessSourceCandidate(IDictionary<string, List<CandidateSourceProperty>> sourceCandidatesByPropertyName,
            string valueName, object source, PropertyInfo sourceProperty, int scoreForMatch, int depth)
        {
            List<CandidateSourceProperty> existingCandidates;

            if (sourceCandidatesByPropertyName.TryGetValue(valueName, out existingCandidates))
            {
                var candidate = new CandidateSourceProperty
                {
                    Property = sourceProperty,
                    Value = sourceProperty.GetValue(source),
                    Score = scoreForMatch,
                    Depth = depth,
                };
                
                existingCandidates.Add(candidate);
            }
        }

        private bool TrySetValue(IDictionary<string, object> propertyValuesByName, string valueName, Func<object> getValue)
        {
            object existingValue;

            if (propertyValuesByName.TryGetValue(valueName, out existingValue) && existingValue == null)
            {
                propertyValuesByName[valueName] = getValue();
                return true;
            }

            return false;
        }

        private string GetRoleName(string propertyName, string propertyTypeName)
        {
            // If the property name matches the type name, there is no role name present 
            if (propertyName.Equals(propertyTypeName, StringComparison.InvariantCultureIgnoreCase))
                return null;

            // Make sure convention is followed... with no suffix
            if (!propertyName.EndsWith(propertyTypeName, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return propertyName.Substring(0, propertyName.Length - propertyTypeName.Length);
        }
    }
}
