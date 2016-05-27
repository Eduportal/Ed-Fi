using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EdFi.Common.SchoolIdentity;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Api.Common.Filters;
using EdFi.Ods.Api.Data.Repositories;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.Api.Models.Resources.School;
using EdFi.Ods.Api.Services.Extensions;
using EdFi.Ods.Pipelines.GetByKey;
using EdFi.Ods.Swagger.Attributes;
using EdFi.Ods.Pipelines.Factories;
using EdFi.Ods.Pipelines.GetMany;
using EntitySchool=EdFi.Ods.Entities.NHibernate.SchoolAggregate.School;

namespace EdFi.Ods.Api.Services.Controllers
{
    [Description("Retrieve or create Unique Ids for a school, and add or update their information")]
    [EdFiAuthorization(Resource = "SchoolIdentity")]
    public class SchoolIdentitiesController : ApiController
    {
        private const string NoIdentitySystem = "There is no integrated Unique Identity System";

        private readonly IUniqueSchoolIdentity _schoolIdentitySubsystem;
        private readonly Lazy<GetByKeyPipeline<School, EntitySchool>> _getByKeyPipeline;
        private readonly Lazy<GetManyPipeline<School, EntitySchool>> _getManyPipeline;

        public SchoolIdentitiesController()
        {

        }

        public SchoolIdentitiesController(IUniqueSchoolIdentity identitySubsystem, IPipelineFactory pipelineFactory)
            : this()
        {
            this._schoolIdentitySubsystem = identitySubsystem;
            this._getByKeyPipeline = new Lazy<GetByKeyPipeline<School, EntitySchool>>(pipelineFactory.CreateGetByKeyPipeline<School, EntitySchool>);
            this._getManyPipeline = new Lazy<GetManyPipeline<School, EntitySchool>>(pipelineFactory.CreateGetManyPipeline<School, EntitySchool>);
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
                var school = new School
                {
                    SchoolId = request.EducationOrganizationId
                };
                
                //Get by SchoolId
                var result = _getByKeyPipeline.Value.Process(new GetByKeyContext<School, EntitySchool>(school, string.Empty));
                if (result.Resource != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, result.Resource.ToResource());
                }
                
                //Get data based on both
                if (!String.IsNullOrEmpty(request.StateOrganizationId) && !String.IsNullOrEmpty(request.NameOfInstitution))
                {
                    school = new School { StateOrganizationId = request.StateOrganizationId, NameOfInstitution = request.NameOfInstitution };
                    var schools = GetSchools(school);
                    if (schools.Any())
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, schools.Select(s => s.ToResource()));
                    }
                }

                var returnData = new List<School>();
                //Get data based on the StateOrganizationId
                if (!String.IsNullOrEmpty(request.StateOrganizationId))
                {
                    school = new School { NameOfInstitution = request.NameOfInstitution };
                    returnData.AddRange(GetSchools(school));
                }

                //Get data based on the name
                if (!String.IsNullOrEmpty(request.NameOfInstitution))
                {
                    school = new School{NameOfInstitution = request.NameOfInstitution};
                    returnData.AddRange(GetSchools(school));
                }

                //Get only unique
                var uniqueReturnData = returnData.GroupBy(s => s.SchoolId).Select(g => g.FirstOrDefault()).Where(r=>r!=null).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, uniqueReturnData.Select(s=>s.ToResource()));
            }
            catch (NotImplementedException)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotImplemented, NoIdentitySystem);
            }
        }

        private List<School> GetSchools(School school)
        {
            var returnData = new List<School>();
            var queryParams = new QueryParameters(new UrlQueryParametersRequest());
            var resultMany = _getManyPipeline.Value.Process(new GetManyContext<School, EntitySchool>(school, queryParams));
            if (resultMany.Resources.Any())
            {
                returnData.AddRange(resultMany.Resources.ToList());
            }
            return returnData;
        }
    }
}