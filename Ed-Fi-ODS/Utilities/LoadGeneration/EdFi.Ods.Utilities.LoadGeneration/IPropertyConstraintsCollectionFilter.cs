using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EdFi.Ods.Utilities.LoadGeneration.Persistence;
using EdFi.Ods.Utilities.LoadGeneration._Extensions;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface IPropertyConstraintsCollectionFilter
    {
        //object IsMatch(object subject, IDictionary<string, object> propertyValueConstraints);
        IEnumerable<object> Filter(IEnumerable<object> source, IDictionary<string, object> propertyValueConstraints);
    }

    public class PropertyConstraintsCollectionFilter : IPropertyConstraintsCollectionFilter
    {
        public IEnumerable<object> Filter(IEnumerable<object> source, IDictionary<string, object> propertyValueConstraints)
        {
            IDictionary<string, PropertyInfo> propertyByName = null;

            foreach (object o in source)
            {
                // Initialize the dictionary
                if (propertyByName == null)
                {
                    propertyByName = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .ToDictionary(p => p.Name, p => p, StringComparer.InvariantCultureIgnoreCase);
                }

                if (propertyValueConstraints.All(c =>
                {
                    PropertyInfo propertyInfo;

                    if (!propertyByName.TryGetValue(c.Key, out propertyInfo))
                        return true;

                    object value = propertyInfo.GetValue(o);

                    if (value == null)
                        return c.Value == null;

                    return value.Equals(c.Value);
                }))
                    yield return o;
            }
        }
    }

    public abstract class UniqueIdPropertyConstraintsCollectionFilterDecoratorBase : IPropertyConstraintsCollectionFilter
    {
        protected ILog _logger;

        private readonly IPropertyConstraintsCollectionFilter _next;
        private readonly IPersonEducationOrganizationCache _personEducationOrganizationCache;

        protected UniqueIdPropertyConstraintsCollectionFilterDecoratorBase(IPropertyConstraintsCollectionFilter next,
            IPersonEducationOrganizationCache personEducationOrganizationCache)
        {
            _next = next;
            _personEducationOrganizationCache = personEducationOrganizationCache;
        }

        protected abstract string PrimaryRelationshipResourceName { get; }
        protected abstract string PersonType { get; }

        public IEnumerable<object> Filter(IEnumerable<object> source, IDictionary<string, object> propertyValueConstraints)
        {
            // Determine the available Ed-Org context for authorization
            var edOrgIdentifiers = propertyValueConstraints
                .Where(x => x.Key.StartsWith("authorization.", StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(x => x.Key.Substring("authorization.".Length), x => x.Value)
                .GetOrderedEducationOrganizationIdentifierValues().ToList();

            // No edorgs in context?  First-pass implementation, we have nothing to do...
            if (!edOrgIdentifiers.Any()) // ||
            //!edOrgIdentifiers.First().Key.Equals("SchoolId", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var o in _next.Filter(source, propertyValueConstraints))
                    yield return o;

                yield break;
            }

            PropertyInfo[] uniqueIdProperties = null;
            PropertyInfo uniqueIdProperty = null;
            IList<string> possiblePeople = null;

            // Given the authorization approach with Ed-Fi, there should only be 1 relevant ed org present for authorization.
            int edOrgId = (int)edOrgIdentifiers.First().Value;

            foreach (object o in _next.Filter(source, propertyValueConstraints))
            {
                // Get properties of first collection instance
                if (uniqueIdProperties == null)
                {
                    uniqueIdProperties =
                        (from p in o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         where p.IsPersonUniqueIdProperty()
                         select p).ToArray();

                    // TODO: This should be captured in a specification class for something like "IsPrimaryAuthorizationRelationship"
                    if (o.GetType().Name != PrimaryRelationshipResourceName)  // TODO: This logic achieves the desired result but logic/reasoning is not clear -- we basically want to skip processing if this is a primary relationship resource, and not setting the property is an expedient, though less obvious, method -- refactor.
                    {
                        uniqueIdProperty =
                            uniqueIdProperties.SingleOrDefault(
                                p => p.Name.Equals(PersonType + "UniqueId", StringComparison.InvariantCultureIgnoreCase));
                    }

                    if (uniqueIdProperty != null)
                        _logger.DebugFormat("Filtering possible existing references with {0}UniqueId using authorization constraints {1}.", PersonType, propertyValueConstraints.ToDebugString());
                }

                // If we are dealing with a unique id
                if (uniqueIdProperty != null)
                {
                    if (possiblePeople == null)
                    {
                        possiblePeople = _personEducationOrganizationCache.GetPeople(PersonType, edOrgId);
                        _logger.DebugFormat("Possible {0}UniqueId values for education organization '{1}' are: [{2}]", PersonType, edOrgId, string.Join(", ", possiblePeople));

                        // Determine if the property constraints have a uniqueId value present... 
                        object contextualUniqueIdValue;
                        if (propertyValueConstraints.TryGetValue(uniqueIdProperty.Name, out contextualUniqueIdValue))
                        {
                            string contextualUniqueId = contextualUniqueIdValue as string;

                            // If the value is not valid due to ed org in context for authorization purposes, 
                            //  then purge current context/constraints of unusable unique id value
                            if (!possiblePeople.Contains(contextualUniqueId))
                            {
                                _logger.DebugFormat("UniqueId value '{0}' is not valid in the current context due to authorization constraints.  Removing it from context.", contextualUniqueId);
                                propertyValueConstraints.Remove(uniqueIdProperty.Name);
                            }
                        }
                    }

                    // This currently will only work in an ideal fashion with incoming with School Ids
                    // In reality, an LEA coming in should be resolved to a list of students at the LEA, but
                    // the first implementation of this will result in none being returned, and a new one will
                    // be created. -- This may also need to be looked at closer, as it is is now being used for 
                    // Staff as well
                    if (possiblePeople.Contains((string)uniqueIdProperty.GetValue(o)))
                        yield return o;
                }
                else
                {
                    yield return o;
                }
            }
        }
    }

    public class StudentUniqueIdPropertyConstraintsCollectionFilterDecorator : UniqueIdPropertyConstraintsCollectionFilterDecoratorBase
    {
        public StudentUniqueIdPropertyConstraintsCollectionFilterDecorator(IPropertyConstraintsCollectionFilter next, IPersonEducationOrganizationCache personEducationOrganizationCache)
            : base(next, personEducationOrganizationCache)
        {
            _logger = LogManager.GetLogger(typeof(StudentUniqueIdPropertyConstraintsCollectionFilterDecorator));
        }

        protected override string PrimaryRelationshipResourceName
        {
            get { return "StudentSchoolAssociation"; }
        }

        protected override string PersonType
        {
            get { return "Student"; }
        }
    }

    public class StaffUniqueIdPropertyConstraintsCollectionFilterDecorator : UniqueIdPropertyConstraintsCollectionFilterDecoratorBase
    {
        public StaffUniqueIdPropertyConstraintsCollectionFilterDecorator(IPropertyConstraintsCollectionFilter next, IPersonEducationOrganizationCache personEducationOrganizationCache)
            : base(next, personEducationOrganizationCache)
        {
            _logger = LogManager.GetLogger(typeof(StaffUniqueIdPropertyConstraintsCollectionFilterDecorator));
        }

        protected override string PrimaryRelationshipResourceName
        {
            get { return "StaffEducationOrganizationEmploymentAssociation"; }
        }

        protected override string PersonType
        {
            get { return "Staff"; }
        }
    }


}