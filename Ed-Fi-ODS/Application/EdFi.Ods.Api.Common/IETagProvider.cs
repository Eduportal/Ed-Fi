using System;

namespace EdFi.Ods.Api.Common
{
    /// <summary>
    /// Defines an interface for obtaining an ETag value from a given entity.
    /// </summary>
    public interface IETagProvider
    {
        string GetETag(object value);

        DateTime GetDateTime(string etag);
    }
}
