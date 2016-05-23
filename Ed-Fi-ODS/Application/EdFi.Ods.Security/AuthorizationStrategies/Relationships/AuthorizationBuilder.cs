using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using EdFi.Common.Extensions;
using EdFi.Common.Security;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;
using log4net;

namespace EdFi.Ods.Security.AuthorizationStrategies.Relationships
{
    /// <summary>
    /// Builds segments of endpoints that must be related for making an authorization decision.
    /// </summary>
    /// <typeparam name="TContextData">The Type of <see cref="RelationshipsAuthorizationContextData"/> to be used in the authorization decision.</typeparam>
    public class AuthorizationBuilder<TContextData>
        where TContextData : RelationshipsAuthorizationContextData
    {
        private readonly TContextData _contextData;
        private readonly IEnumerable<Claim> _relevantClaims;

        private readonly ILog _logger = LogManager.GetLogger(typeof(AuthorizationBuilder<TContextData>));

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationBuilder{TContextData}"/> class using the 
        /// specified <see cref="RelationshipsAuthorizationContextData"/> and collection of <see cref="Claim"/>s.
        /// </summary>
        /// <param name="relevantClaims">The claims that are applicable to the authorization decision.</param>
        /// <param name="educationOrganizationCache">The education organization cache that enables the fast lookup of education organization types from Ids.</param>
        public AuthorizationBuilder(IEnumerable<Claim> relevantClaims, IEducationOrganizationCache educationOrganizationCache)
            : this(educationOrganizationCache)
        {
            _relevantClaims = relevantClaims;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationBuilder{TContextData}"/> class using the 
        /// specified <see cref="RelationshipsAuthorizationContextData"/> and collection of <see cref="Claim"/>s.
        /// </summary>
        /// <param name="relevantClaims">The claims that are applicable to the authorization decision.</param>
        /// <param name="educationOrganizationCache">The education organization cache that enables the fast lookup of education organization types from Ids.</param>
        /// <param name="contextData">The Ed-Fi specific data to be used for authorization.</param>
        public AuthorizationBuilder(IEnumerable<Claim> relevantClaims, IEducationOrganizationCache educationOrganizationCache, TContextData contextData)
            : this(educationOrganizationCache)
        {
            _contextData = contextData;
            _relevantClaims = relevantClaims;
        }

        /// <summary>
        /// Provides shared constructor logic for lazy initialization.
        /// </summary>
        /// <param name="educationOrganizationCache">The education organization cache that enables the fast lookup of education organization types from Ids.</param>
        private AuthorizationBuilder(IEducationOrganizationCache educationOrganizationCache)
        {
            // Lazy initialization
            _claimAuthorizationValues = new Lazy<List<Tuple<string, object>>>(() =>
                GetClaimAuthorizationNameValuePairs(educationOrganizationCache));
        }

        private readonly Lazy<List<Tuple<string, object>>> _claimAuthorizationValues;

        private List<Tuple<string, object>> GetClaimAuthorizationNameValuePairs(
            IEducationOrganizationCache educationOrganizationCache)
        {
            var claimAuthorizationValues = new List<Tuple<string, object>>();

            foreach (Claim claim in _relevantClaims)
            {
                var value = claim.ToEdFiResourceClaimValue();
                    
                if (!value.EducationOrganizationIds.Any())
                    continue;

                foreach (int educationOrganizationId in value.EducationOrganizationIds)
                {
                    var identifiers = educationOrganizationCache.GetEducationOrganizationIdentifiers(
                        educationOrganizationId);

                    // Make sure EdOrg exists on the current ODS connection (where the cache gets its data)
                    if (identifiers == null)
                    {
                        _logger.WarnFormat("Unable to find EducationOrganizationId '{0}' in the education organization cache for the current ODS.",
                            educationOrganizationId);

                        continue;
                    }

                    // TODO: GKM - Embedded convention
                    var claimNameValueTuple = Tuple.Create(
                        identifiers.EducationOrganizationType + "Id",
                        (object)educationOrganizationId);

                    claimAuthorizationValues.Add(claimNameValueTuple);
                }
            }

            return claimAuthorizationValues;
        }

        private readonly List<ClaimsAuthorizationSegment> _claimsAuthorizationSegments = new List<ClaimsAuthorizationSegment>();
        private readonly List<ExistingValuesAuthorizationSegment> _existingValuesAuthorizationSegments = new List<ExistingValuesAuthorizationSegment>();

        /// <summary>
        /// Gets the rules that need to be executed against the source of data for performing the authorization decision.
        /// </summary>
        /// <returns>An enumerable set of AuthorizationRule instances representing the authorization decision.</returns>
        public AuthorizationSegmentCollection GetSegments()
        {
            // Make sure no segments had empty values for the claims.
            if (_claimsAuthorizationSegments.Any(x => !x.ClaimsEndpoints.Any()))
            {
                if (_relevantClaims.Any(x => !x.ToEdFiResourceClaimValue().EducationOrganizationIds.Any()))
                    throw new EdFiSecurityException("The request can not be authorized because there were no education organizations on the claim.");
                else
                    throw new EdFiSecurityException("The request can not be authorized because the education organizations identifiers assigned to the claim did not exist in the underlying ODS.");
            }

            return new AuthorizationSegmentCollection(_claimsAuthorizationSegments, _existingValuesAuthorizationSegments);
        }

        /// <summary>
        /// Adds a rule that indicates the supplied claims must have values that are associated with a property of the Ed-Fi context data.
        /// </summary>
        /// <typeparam name="TResult">The Type of the Ed-Fi context data property referenced in the rule.</typeparam>
        /// <param name="propertySelection">An expression that can be evaluated and executed against the Ed-Fi context data.</param>
        /// <returns>The <see cref="AuthorizationBuilder{TContextData}"/> instance, for chaining methods together.</returns>
        public AuthorizationBuilder<TContextData> ClaimsMustBeAssociatedWith<TResult>(
            Expression<Func<TContextData, TResult>> propertySelection)
        {
            if (_contextData == null)
            {
                var nameAndType = GetPropertyNameAndType(propertySelection);

                _claimsAuthorizationSegments.Add(
                    new ClaimsAuthorizationSegment(
                        _claimAuthorizationValues.Value,
                        new AuthorizationSegmentEndpoint(
                            nameAndType.Item1,
                            nameAndType.Item2)));
            }
            else
            {
                // Combine the referenced value from authorization context with each of the claims
                var nameAndTypeAndValue = GetPropertyNameAndTypeAndValue(propertySelection);

                _claimsAuthorizationSegments.Add(
                    new ClaimsAuthorizationSegment(
                        _claimAuthorizationValues.Value,
                        new AuthorizationSegmentEndpointWithValue(
                            nameAndTypeAndValue.Item1,
                            nameAndTypeAndValue.Item2,
                            nameAndTypeAndValue.Item3)));
            }

            return this;
        }

        public AuthorizationBuilder<TContextData> ClaimsMustBeAssociatedWith(string[] propertyNames)
        {
            if (_contextData == null)
            {
                foreach (string propertyName in propertyNames)
                {
                    var nameAndType = GetPropertyNameAndType(propertyName);

                    _claimsAuthorizationSegments.Add(
                        new ClaimsAuthorizationSegment(
                            _claimAuthorizationValues.Value,
                            new AuthorizationSegmentEndpoint(
                                nameAndType.Item1,
                                nameAndType.Item2)));
                }

            }
            else
            {
                foreach (string propertyName in propertyNames)
                {
                    var nameAndTypeAndValue = GetPropertyNameAndTypeAndValue(propertyName);

                    _claimsAuthorizationSegments.Add(
                        new ClaimsAuthorizationSegment(
                            _claimAuthorizationValues.Value,
                            new AuthorizationSegmentEndpointWithValue(
                                nameAndTypeAndValue.Item1,
                                nameAndTypeAndValue.Item2,
                                nameAndTypeAndValue.Item3)));
                }                
            }

            return this;
        }

        /// <summary>
        /// Adds a rule that indicates that the values of the two Ed-Fi context data properties must be associated.
        /// </summary>
        /// <typeparam name="TResult1">The <see cref="Type"/> of the first Ed-Fi context data property that must be associated.</typeparam>
        /// <typeparam name="TResult2">The <see cref="Type"/> of the second Ed-Fi context data property that must be associated.</typeparam>
        /// <param name="propertyExpression1">The expression that references the first property.</param>
        /// <param name="propertyExpression2">The expression that references the second property.</param>
        /// <returns>The <see cref="AuthorizationBuilder{TContextData}"/> instance, for chaining methods together.</returns>
        public AuthorizationBuilder<TContextData> ExistingValuesMustBeAssociated<TResult1, TResult2>(
            Expression<Func<TContextData, TResult1>> propertyExpression1,
            Expression<Func<TContextData, TResult2>> propertyExpression2)
        {
            if (_contextData == null)
            {
                var nameAndType1 = GetPropertyNameAndType(propertyExpression1);
                var nameAndType2 = GetPropertyNameAndType(propertyExpression2);

                _existingValuesAuthorizationSegments.Add(
                    new ExistingValuesAuthorizationSegment(
                        new AuthorizationSegmentEndpoint(nameAndType1.Item1, nameAndType1.Item2),
                        new AuthorizationSegmentEndpoint(nameAndType2.Item1, nameAndType2.Item2)));
            }
            else
            {
                var nameAndTypeAndValue1 = GetPropertyNameAndTypeAndValue(propertyExpression1);
                var nameAndTypeAndValue2 = GetPropertyNameAndTypeAndValue(propertyExpression2);

                _existingValuesAuthorizationSegments.Add(
                    new ExistingValuesAuthorizationSegment(
                        new AuthorizationSegmentEndpointWithValue(nameAndTypeAndValue1.Item1, nameAndTypeAndValue1.Item2, nameAndTypeAndValue1.Item3),
                        new AuthorizationSegmentEndpointWithValue(nameAndTypeAndValue2.Item1, nameAndTypeAndValue2.Item2, nameAndTypeAndValue2.Item3)));
            }

            return this;
        }

        private Tuple<string, Type> GetPropertyNameAndType(string propertyName)
        {
            var propertyInfo = typeof(TContextData).GetProperty(propertyName);

            if (propertyInfo == null)
            {
                throw new Exception(
                    string.Format("Property '{0}' not found on authorization context data type '{1}'.", 
                    propertyName, typeof(TContextData).Name));
            }

            return Tuple.Create(propertyName, propertyInfo.PropertyType);
        }

        private Tuple<string, Type, object> GetPropertyNameAndTypeAndValue(string propertyName)
        {
            if (propertyName.EqualsIgnoreCase("EducationOrganizationId")
                && _contextData.ConcreteEducationOrganizationIdPropertyName != null)
            {
                propertyName = _contextData.ConcreteEducationOrganizationIdPropertyName;
            }

            var propertyInfo = typeof(TContextData).GetProperty(propertyName);

            if (propertyInfo == null)
            {
                throw new Exception(
                    string.Format("Property '{0}' not found on authorization context data type '{1}'.",
                    propertyName, typeof(TContextData).Name));
            }

            return Tuple.Create(propertyName, propertyInfo.PropertyType, propertyInfo.GetValue(_contextData));
        }

        private Tuple<string, Type> GetPropertyNameAndType<TResult1>(Expression<Func<TContextData, TResult1>> propertyExpression)
        {
            var expr = propertyExpression.Body as MemberExpression;

            if (expr == null || expr.Member.MemberType != MemberTypes.Property)
            {
                throw new ArgumentException(
                    "Argument must be a MemberExpression referencing a property.",
                    "propertyExpression");
            }

            // Get the name of the property being accessed
            return Tuple.Create(expr.Member.Name, expr.Type);
        }

        // By making this static, each closed generic version will have its own accessors
        // which is appropriate if we're dealing with different types of TContextData
        private static ConcurrentDictionary<string, object> propertyAccessorsByName 
            = new ConcurrentDictionary<string, object>();

        private Tuple<string, Type, object> GetPropertyNameAndTypeAndValue<TResult>(
            Expression<Func<TContextData, TResult>> propertySelection)
        {
            var expr = propertySelection.Body as MemberExpression;

            if (expr == null || expr.Member.MemberType != MemberTypes.Property)
            {
                throw new ArgumentException(
                    "Argument must be a MemberExpression referencing a property.",
                    "propertySelection");
            }

            // Get the name of the property being accessed
            string name = expr.Member.Name;

            // Translate resolved EdOrgIds to the appropriate endpoint name
            if (name.EqualsIgnoreCase("EducationOrganizationId")
                && _contextData.ConcreteEducationOrganizationIdPropertyName != null)
            {
                return GetPropertyNameAndTypeAndValue(
                    _contextData.ConcreteEducationOrganizationIdPropertyName);
            }

            // Get (or save) the compiled Func, by name
            var getValue = (Func<TContextData, TResult>) propertyAccessorsByName
                .GetOrAdd(name, n => propertySelection.Compile());

            // Get the value off the authorization context data instance
            object authorizationValue = (_contextData == null) ? default(TResult) : getValue(_contextData);

            // Create a tuple representing the referenced value
            var resultTuple = Tuple.Create(name, expr.Type, authorizationValue);

            return resultTuple;
        }
    }
}