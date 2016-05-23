using System;
using System.Collections.Generic;

#if SUPPORT_MULTIPLE_LINKS_PER_REL

#endif

namespace EdFi.Ods.Entities.Architecture.Hal
{
#if SUPPORT_MULTIPLE_LINKS_PER_REL
    public class Links : Dictionary<string, dynamic>
#else
    public class Links : Dictionary<string, Link>
#endif
    {
        //public Dictionary<string, JsonObject> links = new Dictionary<string, JsonObject>();

        /// <summary>
        /// Gets or sets the link that refers to the resource, to be serialized as the "self" link.
        /// </summary>
        public Link Self
        {
            get
            {
                Link link;
                bool exists = this.TryGetValue("self", out link);
                return link;
            }
            set { this["self"] = value; }
        }

        /// <summary>
        /// Adds a link to the collection using the attributes specified.
        /// </summary>
        /// <param name="rel">Describes the relationship of the targeted resource to the current resource.</param>
        /// <param name="href">The href value of the link.</param>
        /// <param name="title">A title for the link.</param>
        /// <param name="name">A name that can be used as a secondary key for identifying a link with the same relationship.</param>
        /// <param name="type">A hint for the media type expected when dereferencing the target resource.</param>
        /// <param name="profile">A URI which is a hint about the profile of the target resource, as defined by <see cref="http://tools.ietf.org/html/draft-kelly-json-hal-03#ref-I-D.wilde-profile-link">I-D.wilde-profile-link</see>.</param>
        /// <param name="hreflang">The language of the target resource, as defined by <see cref="http://tools.ietf.org/html/rfc5988">RFC5988</see>.</param>
        /// <param name="templated">Indicates whether the <paramref name="href"/> refers to a URI Template (see <see cref="http://tools.ietf.org/html/rfc6570">RFC6570</see>.</param>
        public void AddLink(string rel, string href, string title = null, string name = null, string type = null, string profile = null, string hreflang = null, bool? templated = null)
        {
            // rel is required
            if (string.IsNullOrEmpty(rel))
                throw new ArgumentNullException("rel");

            // href is required
            if (string.IsNullOrEmpty(href))
                throw new ArgumentNullException("href");

            var link = new Link { Href = href };

            // title is optional
            if (!string.IsNullOrEmpty(title))
                link.Title = title;

            // name is optional
            if (!string.IsNullOrEmpty(name))
                link.Name = name;

            // type is optional
            if (!string.IsNullOrEmpty(type))
                link.Type = type;

            // hreflang is optional
            if (!string.IsNullOrEmpty(hreflang))
                link.Hreflang = hreflang;

            // templated is optional
            if (templated != null)
                link.Templated = templated;

            AddLink(rel, link);
        }

        public void AddLink(string rel, Link link)
        {
            // Set "self" links added here to the "self" property
            if (string.Compare("self", rel, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                Self = link;
                return;
            }

#if SUPPORT_MULTIPLE_LINKS_PER_REL
            if (this.ContainsKey(rel))
            {
                dynamic x = this[rel];
                
                if (x.GetType() == typeof(JsonObject))
                {
                    this[rel] = new JsonArrayObjects { x, link };
                }
                else
                {
                    this[rel] = x.Add(link);
                }
            }
            else
            {
                this[rel] = link;
            }
#else
            this[rel] = link;
#endif
        }
    }
}
