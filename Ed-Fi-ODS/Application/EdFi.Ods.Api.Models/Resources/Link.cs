using System.Runtime.Serialization;

namespace EdFi.Ods.Api.Models.Resources
{
    /// <summary>
    /// Contains hyperlink details for a resource.
    /// </summary>
    [DataContract]
    public class Link
    {
        /// <summary>
        /// Gets or sets the description of the relationship of the link to the containing resource.
        /// </summary>
        [DataMember(Name = "rel")]
        public string Rel { get; set; }

        /// <summary>
        /// Gets or sets the hyperlink to the related resource.
        /// </summary>
        [DataMember(Name = "href")]
        public string Href { get; set; }
    }
}
