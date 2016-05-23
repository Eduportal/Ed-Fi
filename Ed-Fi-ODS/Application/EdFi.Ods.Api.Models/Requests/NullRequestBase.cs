using System;
using EdFi.Ods.Common;
using EdFi.Ods.Entities.Common;

namespace EdFi.Ods.Api.Models.Requests
{
    /// <summary>
    /// Provides and abstract base class for concrete "Null" request objects that are generated for
    /// readable-only or writable-only resources in a Profile definition.  The concrete versions for
    /// writable-only should apply an attribute.
    /// </summary>
    public abstract class NullRequestBase : IHasIdentifier, IHasETag
    {
        public Guid Id { get; set; }
        public string ETag { get; set; }
    }
}
