// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Results;
using EdFi.Ods.Api.Architecture;
using EdFi.Ods.Api.Services.Extensions;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Context;
using EdFi.Ods.Common.ExceptionHandling;
using EdFi.Ods.Common.ExceptionHandling.Translators;
using EdFi.Ods.Common.Exceptions;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Pipelines.Delete;
using EdFi.Ods.Pipelines.Factories;
using EdFi.Ods.Pipelines.Get;
using EdFi.Ods.Pipelines.GetByKey;
using EdFi.Ods.Pipelines.GetMany;
using EdFi.Ods.Pipelines.Put;
using EdFi.Ods.Api.Data.Repositories;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Api.Common.CustomActionResults;
using log4net;

namespace EdFi.Ods.Api.Services.Controllers.v2
{
    // TAggregateRoot,  (EdFi.Ods.Entities.NHibernate.StudentAggregate.Student)
    // TResourceModel,  (EdFi.Ods.Api.Models.Resources.Student.Student)
    // TGetByKeyRequest,  (Requests.Students.StudentGetByKey)
    // TGetByIdsRequest,  (Requests.Students.StudentGetByIds)
    // TPostRequest,  (Requests.Students.StudentPost)
    // TPutRequest,  (Requests.Students.StudentPut)
    // TDeleteRequest,  (TDeleteRequest)
    // TPatchRequest (Requests.Students.StudentPatch)

    public abstract class EdFiControllerBase<TResourceReadModel, TResourceWriteModel, TEntityInterface, TAggregateRoot, TGetByKeyRequest, TPutRequest, TPostRequest, TDeleteRequest, TGetByExampleRequest> 
        : ApiController 
        where TResourceReadModel : IHasIdentifier, IHasETag, new() 
        where TResourceWriteModel : IHasIdentifier, IHasETag, new() 
        where TEntityInterface : class
        where TAggregateRoot : class, IHasIdentifier, new()
        where TPutRequest : TResourceWriteModel
        where TPostRequest : TResourceWriteModel
        where TDeleteRequest : IHasIdentifier
    {
        //protected IRepository<TAggregateRoot> repository;
        protected ISchoolYearContextProvider schoolYearContextProvider;
        private readonly IRESTErrorProvider restErrorProvider;

        protected Lazy<PutPipeline<TResourceWriteModel, TAggregateRoot>> putPipeline;
        protected Lazy<DeletePipeline> deletePipeline;
        protected Lazy<GetPipeline<TResourceReadModel, TAggregateRoot>> getByIdPipeline;
        protected Lazy<GetManyPipeline<TResourceReadModel, TAggregateRoot>> getManyPipeline;
        protected Lazy<GetByKeyPipeline<TResourceReadModel, TAggregateRoot>> getByKeyPipeline;

        protected EdFiControllerBase(IPipelineFactory pipelineFactory, ISchoolYearContextProvider schoolYearContextProvider, IRESTErrorProvider restErrorProvider) //IRepository<TAggregateRoot> repository, 
        {
            //this.repository = repository;
            this.schoolYearContextProvider = schoolYearContextProvider;
            this.restErrorProvider = restErrorProvider;
            getByIdPipeline = new Lazy<GetPipeline<TResourceReadModel, TAggregateRoot>>(pipelineFactory.CreateGetPipeline<TResourceReadModel, TAggregateRoot>);
            getByKeyPipeline = new Lazy<GetByKeyPipeline<TResourceReadModel, TAggregateRoot>>(pipelineFactory.CreateGetByKeyPipeline<TResourceReadModel, TAggregateRoot>);
            getManyPipeline = new Lazy<GetManyPipeline<TResourceReadModel, TAggregateRoot>>(pipelineFactory.CreateGetManyPipeline<TResourceReadModel, TAggregateRoot>);
            putPipeline = new Lazy<PutPipeline<TResourceWriteModel, TAggregateRoot>>(pipelineFactory.CreatePutPipeline<TResourceWriteModel, TAggregateRoot>);
            deletePipeline = new Lazy<DeletePipeline>(pipelineFactory.CreateDeletePipeline<TResourceReadModel, TAggregateRoot>);
        }

        private ILog _logger;
        
        protected ILog Logger
        {
            get
            {
                if (_logger == null)
                    _logger = LogManager.GetLogger(GetType());

                return _logger;
            }
        }

        private IHttpActionResult CreateActionResultFromException(Exception exception,
            bool enforceOptimisticLock = false)
        {
            var restError = restErrorProvider.GetRestErrorFromException(exception);
            if (exception is ConcurrencyException && enforceOptimisticLock)
            {
                // See RFC 5789 - Conflicting modification (with "If-Match" header)
                restError.Code = (int) HttpStatusCode.PreconditionFailed;
                restError.Message = "Resource was modified by another consumer.";
            }

            return string.IsNullOrWhiteSpace(restError.Message)
                ? new StatusCodeResult((HttpStatusCode) restError.Code, this)
                : new StatusCodeResult((HttpStatusCode) restError.Code, this).WithError(restError.Message);
        }

        //TODO: this method executes both the GetAll and GetByKey behaviors - this is do to limits with Web Api 1.X routing - post Web Api 2.X upgrade this should be altered to use route attributes on separate methods
        protected abstract void MapKey(TGetByKeyRequest request, TEntityInterface specification);
        protected abstract void MapAll(TGetByExampleRequest request, TEntityInterface specification);

        protected IHttpActionResult GetByKey(TGetByKeyRequest request)
        {
            //TODO: Add support for If-None-Match; current implementation cannot verify value without going to the db
            // Read the incoming ETag header, if present
            string etagValue;
            TryGetRequestHeader("If-None-Match", out etagValue);

            var internalRequestAsResource = new TResourceReadModel();
            var internalRequest = internalRequestAsResource as TEntityInterface;
            MapKey(request, internalRequest);

            var entityInterface = internalRequest as TEntityInterface;
            var isGetByKeyRequest = IsGetByNaturalKey(entityInterface);

            if (!isGetByKeyRequest) return BadRequest("Call to GetByKey without natural key in request");

            var result = getByKeyPipeline.Value.Process((new GetByKeyContext<TResourceReadModel, TAggregateRoot>(internalRequestAsResource, etagValue)));

            // Handle exception result
            if (result.Exception != null)
            {
                Logger.Error("GetByKeyRequest", result.Exception);
                return CreateActionResultFromException(result.Exception);
            }

            if (result.Resource == null)
            {
                return NotFound();
            }

            var resource = result.Resource;
            var okResult = Ok(resource).WithContentType(GetReadContentType());

            return AddOutboundEtagForSingleResult(okResult, resource);
        }

        public virtual IHttpActionResult GetAll([FromUri] UrlQueryParametersRequest urlQueryParametersRequest, [FromUri] TGetByExampleRequest request = default(TGetByExampleRequest))
        {
            //respond quickly to DOS style requests (should we catch these earlier?  e.g. attribute filter?)
            //TODO: actually set an upper limit - this was an off-the-cuff value set by an ignoramus
            if (urlQueryParametersRequest.Limit != null &&
                (urlQueryParametersRequest.Limit <= 0 || urlQueryParametersRequest.Limit > 100))
            {
                return BadRequest("Limit must be omitted or set to a value between 1 to 100");
            }

            var internalRequestAsResource = new TResourceReadModel();
            var internalRequest = internalRequestAsResource as TEntityInterface;
            if (request != null) MapAll(request, internalRequest);

            //TODO: Add support for If-None-Match; current implementation cannot verify value without going to the db
            // Read the incoming ETag header, if present
            string etagValue;
            TryGetRequestHeader("If-None-Match", out etagValue);

            // Execute the pipeline (synchronously)
            var result = getManyPipeline.Value.Process(new GetManyContext<TResourceReadModel, TAggregateRoot>(internalRequestAsResource, new QueryParameters(urlQueryParametersRequest)));

            // Handle exception result
            if (result.Exception != null)
            {
                Logger.Error("GetAllRequest", result.Exception);
                return CreateActionResultFromException(result.Exception);
            }

            // Return multiple results
            return Ok(result.Resources).WithContentType(GetReadContentType());
        }

        public virtual IHttpActionResult Get(Guid id)
        {
            // Read the incoming ETag header, if present
            string etagValue;
            TryGetRequestHeader("If-None-Match", out etagValue);

            // Execute the pipeline (synchronously)
            var result = getByIdPipeline.Value.Process(new GetContext<TAggregateRoot>(id, etagValue));

            // Handle exception result
            if (result.Exception != null)
            {
                Logger.Error("GetByIdRequest", result.Exception);
                return CreateActionResultFromException(result.Exception);
            }

            // Handle success result
            // Add ETag header for the resource
            var resource = result.Resource;
            var okResult = Ok(resource).WithContentType(GetReadContentType());

            return AddOutboundEtagForSingleResult(okResult, resource);
        }

        public virtual IHttpActionResult Put([FromBody] TPutRequest request, [FromUri] Guid id)
        {
            // Manual binding of Id to main request model
            request.Id = id;

            // Read the If-Match header and populate the resource DTO with an etag value.
            string etag;

            bool enforceOptimisticLock = TryGetRequestHeader("If-Match", out etag);

            request.ETag = etag.Unquoted();

            var validationState = new ValidationState();

            // Execute the pipeline (synchronously)
            var result = putPipeline.Value.Process(new PutContext<TResourceWriteModel, TAggregateRoot>(request, validationState));
            // Check for exceptions
            if (result.Exception != null)
            {
                Logger.Error("Put", result.Exception);
                return CreateActionResultFromException(result.Exception, enforceOptimisticLock);
            }

            var status = result.ResourceWasCreated ? HttpStatusCode.Created : HttpStatusCode.NoContent;
            return
                StatusCode(status).With(x =>
                {
                    x.Headers.ETag = GetEtag(result.ETag);
                    x.Headers.Location = new Uri(GetResourceUrl(result.ResourceId.GetValueOrDefault()));
                });
        }

        public virtual IHttpActionResult Post([FromBody] TPostRequest request)
        {
            var validationState = new ValidationState();
            PutResult result;

            // Make sure Id is not already set (no client-assigned Ids)
            if (request.Id != default(Guid))
            {
                result = new PutResult
                {
                    Exception = new ArgumentException("Resource identifiers cannot be assigned by the client.")
                };
            }
            else
            {
                result = putPipeline.Value.Process(new PutContext<TResourceWriteModel, TAggregateRoot>(request, validationState));
            }

            // Throw an exceptions that occurred for global exception handling
            if (result.Exception != null)
            {
                Logger.Error("Post", result.Exception);
                return CreateActionResultFromException(result.Exception);
            }

            //Once here either the resource was created or it already existed and it was updated
            //NEW GUY QUESTION: Is this the expected behavior?  Should a create fail if the resource exists?
            var status = result.ResourceWasCreated ? HttpStatusCode.Created : HttpStatusCode.OK;
            return
                StatusCode(status).With(x =>
                {
                    x.Headers.ETag = GetEtag(result.ETag);
                    x.Headers.Location = new Uri(GetResourceUrl(result.ResourceId.GetValueOrDefault()));
                });
        }

        public IHttpActionResult Delete([FromUri] Guid id)
        {
            // Read the If-Match header and populate the delete context based on the value (or lack of one)
            string etag;

            var enforceOptimisticLock = TryGetRequestHeader("If-Match", out etag);
            etag = etag.Unquoted();

            var deleteContext = enforceOptimisticLock ? new DeleteContext(id, etag) : new DeleteContext(id);

            // Manual binding of Id to main request model
            var result = deletePipeline.Value.Process(deleteContext);

            // Throw an exceptions that occurred for global exception handling
            if (result.Exception != null)
            {
                Logger.Error("Delete", result.Exception);
                return CreateActionResultFromException(result.Exception, enforceOptimisticLock);
            }

            //Return 204 (according to RFC 2616, if the delete action has been enacted but the response does not include an entity, the return code should be 204).
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected virtual string GetReadContentType()
        {
            return "application/json";
        }

        // Support methods
        protected bool TryGetRequestHeader(string headerName, out string value)
        {
            IEnumerable<string> values;
            value = null;

            if (Request == null || !Request.Headers.TryGetValues(headerName, out values))
                return false;

            value = values.FirstOrDefault();

            return !string.IsNullOrEmpty(value);
        }

        protected bool TryProcessEtagHeader<TRequest>(TRequest request, string headerName, Action<TRequest, DateTime> setLastModifiedDate)
        {
            // Check for optimistic locking "opt-in" header value
            IEnumerable<string> values;

            if (!Request.Headers.TryGetValues(headerName, out values))
                return false;

            string etag = values.FirstOrDefault();

            long etagValue = 0;

            if (etag != null && long.TryParse(etag, out etagValue))
            {
                setLastModifiedDate(request, DateTime.FromBinary(etagValue));
                return true;
            }

            return false;
        }

        protected IHttpActionResult AddOutboundEtagForSingleResult(IHttpActionResult response, IHasETag dto)
        {
            var etag = dto.ETag;
            // Add the etag header
            var responseWithEtag = response.With(x => x.Headers.ETag = new EntityTagHeaderValue(etag.Quoted()));

            // Suppress the "_etag" value from the serialized body of the response (to remove redundant data for single responses)
            dto.ETag = null;
            return responseWithEtag;
        }

        private EntityTagHeaderValue GetEtag(string etagValue)
        {
            return new EntityTagHeaderValue(etagValue.Quoted());
        }

        protected abstract bool IsGetByNaturalKey(TEntityInterface request);

        protected abstract string GetResourceCollectionName();

        private static string applicationUrl;

        //TODO: This should be moved to another class and converted to either use a passed in type or generics (and no magic strings - see below)
        protected string GetResourceUrl(Guid id)
        {
            if (applicationUrl == null)
            {
                try { applicationUrl = Request.RequestUri.AbsoluteUri.Split(new string[] { "/api/" }, StringSplitOptions.None)[0]; }
                catch (Exception ex) { throw new Exception("Unable to parse API base URL from request.", ex); }
            }

            return string.Format("{0}/api/v2.0/{1}/{2}/{3}", 
                applicationUrl, 
                schoolYearContextProvider.GetSchoolYear(), 
                GetResourceCollectionName(), 
                id.ToString("n"));
        }
    }
}
