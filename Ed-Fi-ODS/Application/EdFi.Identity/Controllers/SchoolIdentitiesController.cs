using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EdFi.Common.SchoolIdentity;
using EdFi.Identity.Models;
using EdFi.Ods.Api.Common.Filters;
using EdFi.Ods.Swagger.Attributes;
using System;

namespace EdFi.Identity.Controllers
{
    [Description("Retrieve or create Unique Ids for a school, and add or update their information")]
    [EdFiAuthorization(Resource = "SchoolIdentity")]
    public class SchoolIdentitiesController : ApiController
    {
        private const string NoIdentitySystem = "There is no integrated Unique Identity System";
        private readonly IUniqueSchoolIdentity _schoolIdentitySubsystem;
        private readonly ISchoolIdentityMapper _schoolIdentityMapper;

        public SchoolIdentitiesController()
        {

        }

        public SchoolIdentitiesController(IUniqueSchoolIdentity identitySubsystem, ISchoolIdentityMapper schoolIdentityMapper)
            : this()
        {
            this._schoolIdentitySubsystem = identitySubsystem;
            this._schoolIdentityMapper = schoolIdentityMapper;
        }


        /// <summary>
        /// Get is used to lookup an existing Unique Id for a Identity, or suggest possible matches
        /// </summary>
        /// <param name="request">Query by example Identity information</param>
        /// <returns>A list of potential matches for the Identity</returns>
        [HttpGet]
        [ApiRequest(Route = "/SchoolIdentities", Summary = "Lookup an existing Unique Id for a school, or suggest possible matches.", Verb = "Get", Type = typeof(SchoolIdentityResource))]
        [ApiResponseMessage(Code = HttpStatusCode.OK, Message = "One or more Identity matches were found")]
        public HttpResponseMessage GetSchoolsByExample([FromUri] [ApiMember(ParameterType = "query", Name = "request", Description = "School object containing fields values to be searched on", Expand = true)] SchoolIdentityResource request)
        {
            try
            {
                var schoolIdentity = new SchoolIdentity { EducationOrganizationId = 12345 };
                var result = _schoolIdentitySubsystem.Get(schoolIdentity);
                return Request.CreateResponse(HttpStatusCode.OK, result.Select(s => _schoolIdentityMapper.MapToResource(s)));
            }
            catch (NotImplementedException)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotImplemented, NoIdentitySystem);
            }
        }
    }
}