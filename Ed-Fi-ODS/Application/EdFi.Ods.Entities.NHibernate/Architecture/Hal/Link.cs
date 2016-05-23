using System;


namespace EdFi.Ods.Entities.Architecture.Hal
{
    [Serializable]
    public class Link : JsonObject
    {
        /// <summary>
        ///     Gets or sets the href value of the link.
        /// </summary>
        public string Href
        {
            get { return this["href"]; }
            set { this["href"] = value; }
        }

        /// <summary>
        ///     Gets or sets a name that can be used as a secondary key for identifying a link with the same relationship.
        /// </summary>
        public string Name
        {
            get { return this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        ///     Gets or sets the language of the target resource, as defined by <see cref="http://tools.ietf.org/html/rfc5988">RFC5988</see>.
        /// </summary>
        public string Hreflang
        {
            get { return this["hrefLang"]; }
            set { this["hrefLang"] = value; }
        }

        /// <summary>
        ///     Gets or sets a title for the link.
        /// </summary>
        public string Title
        {
            get { return this["title"]; }
            set { this["title"] = value; }
        }

        /// <summary>
        ///     Gets or sets an indication of whether the <see cref="Href" /> refers to a URI Template (see
        ///     <see
        ///         cref="http://tools.ietf.org/html/rfc6570">
        ///         RFC6570
        ///     </see>
        ///     .
        /// </summary>
        public bool? Templated
        {
            get { return this["templated"] == null ? null as bool? : Convert.ToBoolean(this["templated"]); }
            set { this["templated"] = value.ToString().ToLower(); }
        }

        /// <summary>
        ///     Gets or sets a hint for the media type expected when dereferencing the target resource.
        /// </summary>
        public string Type
        {
            get { return this["type"]; }
            set { this["type"] = value; }
        }

        /// <summary>
        ///     Gets or sets a URI which is a hint about the profile of the target resource, as defined by
        ///     <see
        ///         cref="http://tools.ietf.org/html/draft-kelly-json-hal-03#ref-I-D.wilde-profile-link">
        ///         I-D.wilde-profile-link
        ///     </see>
        ///     .
        /// </summary>
        public string Profile
        {
            get { return this["profile"]; }
            set { this["profile"] = value; }
        }

        /// <summary>
        ///     Creates a new <see cref="Link" /> instance and initializes the <see cref="Href" /> property from the assigned string value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Link(string url)
        {
            return new Link {Href = url};
        }
    }
}