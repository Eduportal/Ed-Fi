using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EdFi.Common.SchoolIdentity;
using EdFi.Identity.Models;
using EdFi.Ods.Api.Common.Filters;
using EdFi.Ods.Api.Data.Mappings;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.Api.Models.Resources.School;
using EdFi.Ods.Pipelines.GetByKey;
using EdFi.Ods.Swagger.Attributes;
using EdFi.Ods.Pipelines.Factories;

namespace EdFi.Ods.Api.Services.Controllers
{
    [Description("Retrieve or create Unique Ids for a school, and add or update their information")]
    [EdFiAuthorization(Resource = "SchoolIdentity")]
    public class SchoolIdentitiesController : ApiController
    {
        private const string NoIdentitySystem = "There is no integrated Unique Identity System";

        private readonly IUniqueSchoolIdentity _schoolIdentitySubsystem;
        private Lazy<GetByKeyPipeline<School, Entities.NHibernate.SchoolAggregate.School>> _getByKeyPipeline;


        public SchoolIdentitiesController()
        {

        }

        public SchoolIdentitiesController(IUniqueSchoolIdentity identitySubsystem, IPipelineFactory pipelineFactory)
            : this()
        {
            this._schoolIdentitySubsystem = identitySubsystem;
            this._getByKeyPipeline = new Lazy<GetByKeyPipeline<School, Entities.NHibernate.SchoolAggregate.School>>(pipelineFactory.CreateGetByKeyPipeline<School, Entities.NHibernate.SchoolAggregate.School>);
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
                //Get by key
                var school = new School
                {
                    SchoolId = request.EducationOrganizationId
                };

                var result = _getByKeyPipeline.Value.Process((new GetByKeyContext<School, Entities.NHibernate.SchoolAggregate.School>(school, string.Empty)));
                
                //var schoolIdentity = new SchoolIdentity { EducationOrganizationId = 12345 };
                //var result = _schoolIdentitySubsystem.Get(schoolIdentity);

                return Request.CreateResponse(HttpStatusCode.OK, result.Resource.SchoolId);
            }
            catch (NotImplementedException)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotImplemented, NoIdentitySystem);
            }
        }
    }
}