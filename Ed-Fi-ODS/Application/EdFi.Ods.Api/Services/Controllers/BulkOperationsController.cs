using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using EdFi.Ods.Common.Context;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.Swagger.Attributes;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.Api.Common.Filters;

namespace EdFi.Ods.Api.Services.Controllers
{
    [Description("Manage bulk operation sessions")]
    [Authorize]
    public class BulkOperationsController : ApiController
    {
        private readonly ICreateBulkOperationAndGetById _createBulkOperationAndGetById;
        private readonly ICreateBulkOperationCommandFactory _createBulkOperationCommandFactory;
        private readonly ISchoolYearContextProvider _schoolYearContextProvider;

        public BulkOperationsController(ICreateBulkOperationAndGetById createBulkOperationAndGetById, ICreateBulkOperationCommandFactory createBulkOperationCommandFactory, 
            ISchoolYearContextProvider schoolYearContextProvider)
        {
            _createBulkOperationAndGetById = createBulkOperationAndGetById;
            _createBulkOperationCommandFactory = createBulkOperationCommandFactory;
            _schoolYearContextProvider = schoolYearContextProvider;
        }

        [ApiRequest(Summary = "Create a bulk operation session and retrieve an operation identifier", Notes = "This creates a session, during which XML interchange files are uploaded, committed, and processed. An Operation Identifier is returned that will be used for future calls.", Verb = "Post", Type = typeof(BulkOperationCreateRequest))]
        public IHttpActionResult Post([FromBody]
            [ApiMember(ParameterType = "body", Name = "body")]
            BulkOperationCreateRequest request)
        {
            var command = _createBulkOperationCommandFactory.Create(request);

            if (command.Invalid)
            {
                var errorMessage = ComposeErrorMessage(command);
                return BadRequest(errorMessage);
            }

            _createBulkOperationAndGetById.Execute(command);

            Trace.TraceInformation("Created bulk operation with ID '{0}'", command.Resource.Id);
            return CreatedAtRoute("DefaultApi", new { controller = "bulkoperations", id = command.Resource.Id }, command.Resource);
        }

        private static string ComposeErrorMessage(CreateBulkOperationCommand command)
        {
            return command.ValidationErrors
                               .Select(x => x)
                               .Aggregate((current, next) => current + ", " + next);
        }

        [ApiRequest(Summary = "Get a bulk operation resource that matches the operation identifier", Verb = "Get", Type = typeof(BulkOperationResource))]
        public IHttpActionResult Get(
            [ApiMember(ParameterType = "path", Description = "The operation identifier")]string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var bulkOperation = _createBulkOperationAndGetById.GetByIdAndYear(id,
                _schoolYearContextProvider.GetSchoolYear());
    
            if (bulkOperation == null)
            {
               return NotFound();
            }

            return Ok(bulkOperation);
        }
    }
}
