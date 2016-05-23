using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using EdFi.Common.Messaging;
using EdFi.Ods.Api.Common.Filters;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.Messaging.BulkLoadCommands;
using EdFi.Ods.Swagger.Attributes;
using EdFi.Ods.Api.Services.Filters;


namespace EdFi.Ods.Api.Services.Controllers
{
    [Description("Upload interchange XML files")]
    [Authorize]
    [RoutePrefix("api/v{apiVersion}/{schoolYearFromRoute}/uploads/{uploadId:guid}")]
    public class UploadsController : ApiController
    {
        private readonly IVerifyUploads _verifyUploads;
        private readonly Func<string, MultipartStreamProvider> _createChunkWriter;
        private readonly ICommandSender _commandSender;
        private readonly IBulkOperationsFileChunkCreator _bulkOperationsFileChunkCreator;

        public UploadsController(IVerifyUploads verifyUploads, Func<string, MultipartStreamProvider> createChunkWriter, 
            ICommandSender commandSender, IBulkOperationsFileChunkCreator bulkOperationsFileChunkCreator)
        {
            _verifyUploads = verifyUploads;
            _createChunkWriter = createChunkWriter;
            _commandSender = commandSender;
            _bulkOperationsFileChunkCreator = bulkOperationsFileChunkCreator;
        }

        [ApiRequest(Route = "/uploads/{uploadid}/chunk", Summary = "Allows for the upload of files parts of a larger upload file.", Verb = "Post", Type = typeof(string))]
        [HttpPost]
        [MustBeMultipartContent]
        [Route("chunk")]
        public async Task<IHttpActionResult> PostChunk(
            [ApiMember(ParameterType = "path", Name = "uploadId", Description = "uploadId (string): required The unique ID of the in-progress upload on the server. This value should be obtained from the operation created via the bulk operations API")]
            string uploadId,

            [ApiMember(ParameterType = "query", Name = "offset", Description = "The byte offset of this chunk, relative to the beginning of the full file. This value will be used along with the total expected file size and the bytes value to validate the action. If the offset + bytes > expected bytes or if the bytes received do not match the bytes expected (for the chunk) a 400 Bad Request response will be sent.")]
            [FromUri]
            long offset,

            [ApiMember(ParameterType = "query", Name = "size", Description = "The total bytes for this chunk. This value cannot exceed 157286400. " +
                         "If compression is used this should be compressed bytes and not uncompressed bytes")]
            [FromUri]
            long size)
        {
            var errors = _verifyUploads.IsValid(uploadId, offset, size);
            if (errors.Any)
            {
                return BadRequest(errors.ErrorMessage);
            }

            try
            {
                // ------------------------------------------------------------------------------------------------
                //                        READ before you alter the next line !!!!!!!
                // The framework provides a facility for reading data from a request stream but, sadly, it has a
                // lot of static behavior and expects a MultipartStreamProvider + HttpContext . . . grrrrr
                // Unless you have a complete replacement for this behavior, please be sure you understand the full
                // behavior of the framework code 
                // (https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/src/System.Net.Http.Formatting/HttpContentMultipartExtensions.cs) 
                // and our custom MultipartStreamProvider before you make any changes.
                // ------------------------------------------------------------------------------------------------
                var id = _bulkOperationsFileChunkCreator.CreateChunk(uploadId, offset, size);
                var sizeEquals = await Request.Content.ReadAsMultipartAsync(_createChunkWriter(id)).ContinueWith(t => _bulkOperationsFileChunkCreator.VerifyChunkSize(id, size));
                
                return sizeEquals
                    ? (IHttpActionResult) StatusCode(HttpStatusCode.Created)
                    : BadRequest("Uploaded chunk does not match file size indicated.");
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [ApiRequest(Summary = "Creates a Command to commit the uploaded chunks and validate the file appears composable.", Route = "/Uploads/{uploadid}/Commit", Type = typeof(string))]
        [HttpPost]
        [Route("commit")]
        public IHttpActionResult PostCommit(
            [ApiMember(ParameterType = "path", Name = "uploadid", Description = "The id of the upload to commit to the bulk operation.")]
            string uploadId)
        {
            Trace.TraceInformation(string.Format("Received commit for upload '{0}'", uploadId));
            var errors = _verifyUploads.IsValid(uploadId);
            if (errors.Any)
            {
                return BadRequest(errors.ErrorMessage);
            }

            var cmd = new CommitUploadCommand { UploadId = uploadId, CommitedOn = DateTime.UtcNow };

            Trace.TraceInformation("Sending CommitUploadCommand with upload ID '{0}'", uploadId);
            _commandSender.Send(cmd);
            return StatusCode(HttpStatusCode.Accepted);
        }
    }
}
