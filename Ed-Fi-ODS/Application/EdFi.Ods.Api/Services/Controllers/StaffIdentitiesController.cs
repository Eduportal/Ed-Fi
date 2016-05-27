using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EdFi.Common.StaffIdentity;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Api.Common.Filters;
using EdFi.Ods.Api.Data.Repositories;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.Api.Models.Resources.Staff;
using EdFi.Ods.Api.Services.Extensions;
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
        private readonly Lazy<GetManyPipeline<Staff, EntityStaff>> _getManyPipeline;
        public StaffIdentitiesController()
        {

        }

        public StaffIdentitiesController(IUniqueStaffIdentity identitySubsystem, IPipelineFactory pipelineFactory)
            : this()
        {
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
                Staff staff;
                var returnData = new List<Staff>();
                
                //Get by SchoolId
                if (!String.IsNullOrEmpty(request.StaffUniqueId))
                {
                    staff = new Staff { StaffUniqueId = request.StaffUniqueId};
                    returnData =  GetStaffMembers(staff);
                    if (returnData.Any())
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, returnData.Select(s => s.ToResource()));
                    }
                }

                if (!String.IsNullOrEmpty(request.FirstName) && !String.IsNullOrEmpty(request.LastSurname))
                {
                    staff = new Staff { FirstName = request.FirstName, LastSurname = request.LastSurname };
                    returnData = GetStaffMembers(staff);
                    if (returnData.Any())
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, returnData.Select(s => s.ToResource()));
                    }
                }

                
                if (!String.IsNullOrEmpty(request.FirstName))
                {
                    staff = new Staff{FirstName = request.FirstName};
                    returnData = GetStaffMembers(staff);
                }

                if (!String.IsNullOrEmpty(request.LastSurname))
                {
                    staff = new Staff{LastSurname = request.LastSurname};
                    returnData = GetStaffMembers(staff);
                }

                return Request.CreateResponse(HttpStatusCode.OK, returnData.Select(s => s.ToResource()));
            }
            catch (NotImplementedException)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotImplemented, NoIdentitySystem);
            }
        }

        private List<Staff> GetStaffMembers(Staff staff)
        {
            var queryParams = new QueryParameters(new UrlQueryParametersRequest());
            var returnData=new List<Staff>();
            var resultMany = _getManyPipeline.Value.Process(new GetManyContext<Staff, EntityStaff>(staff, queryParams));
            if (resultMany.Resources.Any())
            {
                returnData.AddRange(resultMany.Resources.ToList());
            }
            return returnData;
        }
    }
}