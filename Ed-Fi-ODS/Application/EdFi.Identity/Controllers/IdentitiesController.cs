using System;
using EdFi.Ods.Api.Common.Filters;

namespace EdFi.Identity.Controllers
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using EdFi.Common.Identity;
    using EdFi.Identity.Models;
    using EdFi.Ods.Swagger.Attributes;

    using FluentValidation;

    /// <summary>
    /// Manage Identity identity, create and lookup People from an external unique identity system
    /// </summary>
    /// <remarks>
    /// Register a custom IUniqueIdentity for a specific implementation, 
    /// or use the UnimplementedUniqueIdentity class if there is no unique identity system.
    /// </remarks>
    [Description("Retrieve or create Unique Ids for a Identity, and add or update their information")]
    [EdFiAuthorization(Resource = "Identity")]
    public class IdentitiesController : ApiController
    {
        private const string NoIdentitySystem = "There is no integrated Unique Identity System";
        private readonly IUniqueIdentity _identitySubsystem;
        private readonly IIdentityMapper _identityMapper;
        private readonly IValidator<IdentityResource> _validator;

        public IdentitiesController(IUniqueIdentity identitySubsystem, IIdentityMapper identityMapper, IValidator<IdentityResource> validator)
        {
            this._identitySubsystem = identitySubsystem;
            this._identityMapper = identityMapper;
            this._validator = validator;
        }

        /// <summary>
        /// Return a single Identity by their Unique Id
        /// </summary>
        /// <param name="id">an Unique Id</param>
        /// <returns>200 and a single Identity or 404 and no data</returns>
        [HttpGet]
        [ApiRequest(Route = "/Identities/{id}", Summary = "Retrieve a single person record from their Unique Id.", Verb = "Get", Type = typeof(string))]
        [ApiResponseMessage(Code = HttpStatusCode.OK, Message = "The requested Identity matched the provided Unique Id.", ResponseModel = typeof(IdentityResource))]
        [ApiResponseMessage(Code = HttpStatusCode.NotFound, Message = "No Identity matching the provided Unique Id was found.", ResponseModel = typeof(IdentityResource))]
        public HttpResponseMessage GetById(
            [ApiMember(ParameterType = "path", Name = "id", Description = "Unique Id of the person to be retrieved")]
            string id)
        {
            try
            {
                var result = _identitySubsystem.Get(id);
                if (result == null)
                {
                    var response = this.Request.CreateResponse<IIdentity>(HttpStatusCode.NotFound, null);
                    response.ReasonPhrase = "No Identity matching the provided Unique Id was found";
                    return response;
                }
                else
                    return this.Request.CreateResponse(HttpStatusCode.OK, this._identityMapper.MapToResource(result));
            }
            catch (NotImplementedException)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotImplemented, NoIdentitySystem);
            }
        }

        /// <summary>
        /// Get is used to lookup an existing Unique Id for a Identity, or suggest possible matches
        /// </summary>
        /// <param name="request">Query by example Identity information</param>
        /// <returns>A list of potential matches for the Identity</returns>
        [HttpGet]
        [ApiRequest(Route = "/Identities",
            Summary = "Lookup an existing Unique Id for a person, or suggest possible matches.",
            Verb = "Get", Type = typeof (IdentityResource))]
        [ApiResponseMessage(Code = HttpStatusCode.OK, Message = "One or more Identity matches were found")]
        public HttpResponseMessage GetByExample(
            [FromUri] [ApiMember(ParameterType = "query", Name = "request",
                Description = "Person object containing fields values to be searched on",
                Expand = true)] IdentityResource request)
        {
            try
            {
                var result = _identitySubsystem.Get(_identityMapper.MapToModel(request));
                return Request.CreateResponse(HttpStatusCode.OK,
                    result.Select(i => _identityMapper.MapToResource(i)).ToList());
            }
            catch (NotImplementedException)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotImplemented, NoIdentitySystem);
            }
        }


        /// <summary>
        /// Creates a new Unique Id for the given Identity information. 
        /// Assumption here is that the user has verified that possible matches are not correct matches
        /// </summary>
        /// <param name="request">Identity information without Unique Id</param>
        /// <returns>Completed Identity information with Unique Id</returns>
        [HttpPost]
        [ApiRequest(Route = "/Identities", Summary = "Creates a new Unique Id for the given Identity information. Assumption here is that the user has verified that possible matches are not correct matches.",
            Verb = "Post", Notes = "A new identifier will be created", Type = typeof(IdentityResource))]
        [ApiResponseMessage(Code = HttpStatusCode.OK, Message = "A Identity was created. The new Unique Id is returned in the returned Identity record")]
        public IHttpActionResult Post(
            [ApiMember(ParameterType = "body", Name = "request", Description = "Identity object to be created")]
            IdentityResource request)
        {
            try
            {
                var validationErrors = this.ValidateIdentity(request).ToArray();

                if (validationErrors.Any())
                {
                    var errorMessage = validationErrors
                        .Select(x => x)
                        .Aggregate((current, next) => current + ", " + next);

                    return this.BadRequest(errorMessage);
                }
                var result = this._identitySubsystem.Post(this._identityMapper.MapToModel(request));
                return this.Ok(this._identityMapper.MapToResource(result));
            }
            catch (NotImplementedException)
            {
                return this.Content(HttpStatusCode.NotImplemented, NoIdentitySystem);
            }
        }

        private IEnumerable<string> ValidateIdentity(IdentityResource identityResource)
        {
            return this._validator.Validate(identityResource).Errors.Select(x => x.ErrorMessage);
        }
    }
}
