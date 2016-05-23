using RestSharp;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    /// <summary>
    /// Provides methods for obtaining and releasing <see cref="IRestClient"/> instances for
    /// use in invoking the REST API SDK.
    /// </summary>
    public interface IRestClientPool
    {
        /// <summary>
        /// Gets a REST client from the pool.
        /// </summary>
        /// <param name="withSchoolYear">Indicates whether the REST client should be initialized with a base URL that includes the school year.</param>
        /// <returns>A REST client that should be used and subsequently released.</returns>
        IRestClient GetRestClient(bool withSchoolYear);

        /// <summary>
        /// Releases a REST client for use by another thread.
        /// </summary>
        /// <param name="restClient">The REST client to be released.</param>
        /// <returns><b>true</b> if the REST client was found in the pool and released; otherwise <b>false</b>.</returns>
        bool ReleaseRestClient(IRestClient restClient);
    }
}