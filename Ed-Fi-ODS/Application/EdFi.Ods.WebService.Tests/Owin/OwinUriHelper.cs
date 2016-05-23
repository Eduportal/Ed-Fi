namespace EdFi.Ods.WebService.Tests.Owin
{
    internal class OwinUriHelper
    {
        internal static string BuildApiUri(string schoolYear, string resourceName, string queryString = null)
        {
            return string.Format("http://owin/api/v2.0/{0}{1}{2}", (string.IsNullOrWhiteSpace(schoolYear) ? "" : schoolYear + "/"), resourceName, (string.IsNullOrWhiteSpace(queryString)) ? "" : "?" + queryString);
        }

        internal static string CreateChunksUri(string suppliedUploadId, int suppliedOffset, int suppliedChunkSize)
        {
            return string.Format("http://owin/api/v2.0/2014/Uploads/{0}/chunk?offset={1}&size={2}", suppliedUploadId, suppliedOffset, suppliedChunkSize);
        }

        internal static string CreateCommitUri(string suppliedUploadId)
        {
            return string.Format("http://owin/api/v2.0/2014/Uploads/{0}/commit", suppliedUploadId);
        }
    }
}
