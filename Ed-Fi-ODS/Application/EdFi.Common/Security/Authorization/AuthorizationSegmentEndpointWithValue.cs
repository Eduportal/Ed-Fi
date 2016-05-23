using System;

namespace EdFi.Common.Security.Authorization
{
    /// <summary>
    /// Represents an authorization segment endpoint with a value.
    /// </summary>
    public class AuthorizationSegmentEndpointWithValue : AuthorizationSegmentEndpoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationSegmentEndpoint"/> class using the specified name, type and value.
        /// </summary>
        /// <param name="name">The name of the rule value (e.g. a property name from the <see cref="EdFiAuthorizationContextData"/>).</param>
        /// <param name="type">The <see cref="Type"/> of the rule value.</param>
        /// <param name="value">The value of the endpoint.</param>
        public AuthorizationSegmentEndpointWithValue(string name, Type type, object value)
            : base(name, type)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the value of the rule value.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Returns the <see cref="AuthorizationSegmentEndpoint.Name"/> and <see cref="Value"/> of the segment endpoint.
        /// </summary>
        /// <returns>The endpoint's name and value.</returns>
        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Value);
        }
    }
}