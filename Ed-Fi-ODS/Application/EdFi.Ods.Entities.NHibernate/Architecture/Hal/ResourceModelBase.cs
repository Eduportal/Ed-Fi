using System;
using System.Runtime.Serialization;

namespace EdFi.Ods.Entities.Architecture.Hal
{
    /// <summary>
    /// Provides a base class for models to be serialized to JSON using the HAL media type. 
    /// See http://stateless.co/hal_specification.html for more information.
    /// </summary>
    [Serializable]
    public abstract class ResourceModelBase
    {
        protected ResourceModelBase()
        {
            Links = new Links();
        }

        /// <summary>
        /// Gets or sets the "self" Url that can be used access the resource (short for "Links.Self").
        /// </summary>
        [IgnoreDataMember]
        public virtual string Url
        {
            get
            {
                Link self = Links.Self;

                return self == null ? null : self.Href;
            }
            set { Links.Self = value; }
        }

        /// <summary>
        /// Gets the collection of other links related to the resource.
        /// </summary>
        [IgnoreDataMember] // Don't use ServiceStack to serialize this.
        public virtual Links Links { get; private set; }

        /// <summary>
        /// Returns the Links property if links have been defined, otherwise <b>null</b> (enables ServiceStack JSON serializer to omit empty "_links").
        /// </summary>
        [Obsolete("Use the Link property instead.  This is here only for ServiceStack JSON serialization.")]
        public virtual Links _links { get { return Links.Count > 0 ? Links : null; } }
    }
}
