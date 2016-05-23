using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using EdFi.Common.Extensions;
using EdFi.Ods.Api.Services.Extensions;

namespace EdFi.Ods.Api.Services.Metadata
{
    public class MetadataController : ApiController
    {
        private const string EmptyContent = @"{""apiVersion"": """", ""swaggerVersion"":"""", ""basePath"": """", ""resourcePath"":"""", ""produces"": """", ""apis"" : """", ""models"": """"}";
        
        public async Task<HttpResponseMessage> Get(string section, string id = "api-docs")
        {
            var content = await Task.Run<StringContent>(
                () =>
                {
                var baseUrl = HttpRuntime.AppDomainAppVirtualPath == "/"
                    ? Request.RequestUri.GetLeftPart(UriPartial.Authority)
                    : Request.RequestUri.GetLeftPart(UriPartial.Authority) + HttpRuntime.AppDomainAppVirtualPath;

                    var filename = Path.ChangeExtension(id, "json");
                    string resourceItemName = string.Format("{0}.{1}", section, filename);


                    string metadataContent;

                    if (!SwaggerMetadataCache.MetadataByResourceItemName.TryGetValue(resourceItemName, out metadataContent))
                        return new StringContent(EmptyContent);

                    if (string.IsNullOrWhiteSpace(metadataContent))
                        return new StringContent(EmptyContent);

                    return new StringContent(
                        metadataContent
                            .Replace("%BASE_URL%", baseUrl)
                            .Replace("%SECTION%", section));
                });

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return result;
        }

        // This static array is used to establish a fixed order of these "standard" API sections
        private static readonly string[] StandardSections = { "Resources", "Descriptors", "Types", "Other" };

        public async Task<HttpResponseMessage> Get()
        {
            var etagValue = SwaggerMetadataCache.SwaggerMetadataHash.ToString();

            // Handle ETag-based cache optimization
            IEnumerable<string> ifNoneMatchValues;

            if (Request.Headers.TryGetValues("If-None-Match", out ifNoneMatchValues))
            {
                if (etagValue == ifNoneMatchValues.First().Unquoted())
                    return new HttpResponseMessage(HttpStatusCode.NotModified);
            }

            var content = await Task.Run<StringContent>(
                () =>
                {
                    var allSectionNames =
                        (from k in SwaggerMetadataCache.MetadataByResourceItemName.Keys
                         select k.Split('.')[0])
                            .Distinct()
                            .OrderBy(x => x);

                    // Create list of "standard" API sections for which metadata was found (ordered according to the StandardSections array)
                    var standardSectionJsonObjects =
                        StandardSections.Intersect(allSectionNames, StringComparer.InvariantCultureIgnoreCase)
                        .Select(x => GetSectionDetailsAsJsonObject(x, true))
                        .ToList();

                    // Create list of "profile" API sections for which metadata was found, ordered by name
                    var profileSectionJsonObjects =
                        allSectionNames.Except(StandardSections, StringComparer.InvariantCultureIgnoreCase)
                        .Select(x => GetSectionDetailsAsJsonObject(x, false))
                        .ToList();
                    
                    return new StringContent(
                        "["
                        + string.Join(",", standardSectionJsonObjects.Concat(profileSectionJsonObjects))
                        + "]");
                });

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content, 
            };

            result.Headers.ETag = new EntityTagHeaderValue(etagValue.DoubleQuoted());
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return result;
        }

        private static string GetSectionDetailsAsJsonObject(string sectionName, bool isStandardSection)
        {
            return string.Format(
                "{{\"displayName\":\"{0}\",\"value\":\"{1}\",\"isStandardSection\":{2}}}",
                sectionName.NormalizeCompositeTermForDisplay('-'),
                sectionName,
                isStandardSection.ToString().ToLower());
        }
    }
}