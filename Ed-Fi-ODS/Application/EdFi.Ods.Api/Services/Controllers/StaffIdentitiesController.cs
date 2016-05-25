using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EdFi.Common.SchoolIdentity;
using EdFi.Common.StaffIdentity;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Api.Common.Filters;
using EdFi.Ods.Api.Data.Repositories;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.Api.Models.Resources.School;
using EdFi.Ods.Api.Models.Resources.Staff;
using EdFi.Ods.Api.Services.Extensions;
using EdFi.Ods.Pipelines.GetByKey;
using EdFi.Ods.Swagger.Attributes;
using EdFi.Ods.Pipelines.Factories;
using EdFi.Ods.Pipelines.GetMany;
using EntityStaff = EdFi.Ods.Entities.NHibernate.StaffAggregate.Staff;

namespace EdFi.Ods.Api.Services.Controllers
{
    [Description("Retrieve or create Unique Ids for a staff, and add or update their information")]
    [EdFiAuthorization(Resource = "StaffIdentity")]
    public class StaffIdentitiesController : ApiController
    {
        private const string NoIdentitySystem = "There is no integrated Unique Identity System";
        private readonly Lazy<GetByKeyPipeline<Staff, EntityStaff>> _getByKeyPipeline;
        private readonly Lazy<GetManyPipeline<Staff, EntityStaff>> _getManyPipeline;

        public StaffIdentitiesController()
        {

        }

        public StaffIdentitiesController(IUniqueStaffIdentity identitySubsystem, IPipelineFactory pipelineFactory)
            : this()
        {
            this._getByKeyPipeline = new Lazy<GetByKeyPipeline<Staff, EntityStaff>>(pipelineFactory.CreateGetByKeyPipeline<Staff, EntityStaff>);
            this._getManyPipeline = new Lazy<GetManyPipeline<Staff, EntityStaff>>(pipelineFactory.CreateGetManyPipeline<Staff, EntityStaff>);
        }


        /// <summary>
        /// Get is used to lookup an existing Unique Id for a Identity, or suggest possible matches
        /// </summary>
        /// <param name="request">Query by example Identity information</param>
        /// <returns>A list of potential matches for the Identity</returns>
        [HttpGet]
        [ApiRequest(Route = "/StaffIdentities", Summary = "Lookup an existing Unique Id for a staff, or suggest possible matches.", Verb = "Get", Type = typeof(StaffIdentityResource))]
        [ApiResponseMessage(Code = HttpStatusCode.OK, Message = "One or more Identity matches were found")]
        public HttpResponseMessage GetStaffByExample([FromUri] [ApiMember(ParameterType = "query", Name = "request", Description = "Staff object containing fields values to be searched on", Expand = true)] StaffIdentityResource request)
        {
            try
            {
                var staff = new Staff
                {
                    StaffUniqueId = request.StaffUniqueId
                };
                var queryParams = new QueryParameters(new UrlQueryParametersRequest());

                //Get by SchoolId
                var result = _getManyPipeline.Value.Process(new GetManyContext<Staff, EntityStaff>(staff, queryParams));
                if (result.Resources.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, result.Resources.Select(s => s.ToResource()));
                }

                var returnData = new List<Staff>();

                //Get data based on the firstname
                if (!String.IsNullOrEmpty(request.FirstName))
                {
                    staff = new Staff { FirstName = request.FirstName };
                    var resultMany = _getManyPipeline.Value.Process(new GetManyContext<Staff, EntityStaff>(staff, queryParams));
                    if (resultMany.Resources.Any())
                    {
                        returnData.AddRange(resultMany.Resources.ToList());
                    }
                }

                //Get data based on the name
                if (!String.IsNullOrEmpty(request.LastSurname))
                {
                    staff = new Staff
                    {
                        LastSurname = request.LastSurname

                    };
                    var getManyContext = new GetManyContext<Staff, EntityStaff>(staff, queryParams);
                    var resultMany = _getManyPipeline.Value.Process(getManyContext);
                    if (resultMany.Resources.Any())
                    {
                        returnData.AddRange(resultMany.Resources.ToList());
                    }
                }

                //Get data based on both
                if (!String.IsNullOrEmpty(request.FirstName) && !String.IsNullOrEmpty(request.LastSurname))
                {
                    staff = new Staff { FirstName = request.FirstName, LastSurname = request.LastSurname };
                    var resultMany = _getManyPipeline.Value.Process(new GetManyContext<Staff, EntityStaff>(staff, queryParams));
                    if (resultMany.Resources.Any())
                    {
                        returnData.AddRange(resultMany.Resources.ToList());
                    }
                }

                //Get only unique
                var uniqueReturnData = returnData.GroupBy(s => s.Id).Select(g => g.FirstOrDefault()).Where(r => r != null).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, uniqueReturnData.Select(s => s.ToResource()));
            }
            catch (NotImplementedException)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotImplemented, NoIdentitySystem);
            }
        }
    }
}