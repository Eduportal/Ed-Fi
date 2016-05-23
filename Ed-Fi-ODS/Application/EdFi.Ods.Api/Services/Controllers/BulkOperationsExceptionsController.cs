using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EdFi.Ods.Api.Data.Repositories.BulkOperations.Exceptions;
using EdFi.Ods.Api.Data.Repositories;
using EdFi.Ods.Swagger.Attributes;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.Api.Common.Filters;

namespace EdFi.Ods.Api.Services.Controllers
{
    [Description("Retrieve bulk operations exceptions")]
    [Authorize]
    [RoutePrefix("api/v{apiVersion}/{schoolYearFromRoute}/bulkoperations/{id:guid}/exceptions")]
    public class BulkOperationsExceptionsController : ApiController
    {
        private readonly IBulkOperationsExceptionsGetByUploadFileId _bulkOperationsExceptionsGetByUploadFileId;

        public BulkOperationsExceptionsController(
            IBulkOperationsExceptionsGetByUploadFileId bulkOperationsExceptionsGetByUploadFileId)
        {
            _bulkOperationsExceptionsGetByUploadFileId = bulkOperationsExceptionsGetByUploadFileId;
        }

        [ApiRequest(Route = "{uploadid}", Summary = "Retrieves collection of exceptions from a bulk operation.", Verb = "Get", Type = typeof(BulkOperationException[]))]
        [HttpGet]
        [Route("{uploadId:guid}")]
        public HttpResponseMessage Get(
            [ApiMember(ParameterType = "path", Name = "id", Description = "id (string): required The unique ID of the operation. This value should be obtained from the operation created via the bulk operations API")]
            string id,
            [ApiMember(ParameterType = "path", Name = "uploadid", Description = "uploadId (string): required The unique ID of the in-progress upload on the server. This value should be obtained from the operation created via the bulk operations API")]
            string uploadId,
            [ApiMember(ParameterType = "query", Name = "offset", Description = "Indicates how many items should be skipped before returning results.")]
            int? offset = null,
            [ApiMember(ParameterType = "query", Name = "limit", Description = "Indicates the maximum number of items that should be returned in the results (defaults to 25).")]
            int? limit = null)
        {
            if (limit != null &&
                (limit == 0 || limit > 100))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Limit must be omitted or set to a value between 1 to 100");
            }

            var exceptions = _bulkOperationsExceptionsGetByUploadFileId.GetByUploadFileId(uploadId, new QueryParameters { Offset = offset, Limit = limit });
            return Request.CreateResponse(HttpStatusCode.OK, exceptions);
        }
    }
}