using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using EdFi.Ods.Common.Context;

namespace EdFi.Ods.Api.Services.Filters
{
    public class RouteValuesContextFilter : ActionFilterAttribute
    {
        private readonly ISchoolYearContextProvider schoolYearContextProvider;

        public RouteValuesContextFilter(ISchoolYearContextProvider schoolYearContextProvider)
        {
            this.schoolYearContextProvider = schoolYearContextProvider;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            object schoolYearAsObject;
            
            // Try to get the School Year value from the current route data
            if (!actionContext.RequestContext.RouteData.Values.TryGetValue("schoolYearFromRoute", out schoolYearAsObject)) 
                return;

            // Prevent null reference exceptions
            if (schoolYearAsObject == null)
                return;

            int schoolYear;
            
            // Convert the object value to a string and try to parse it
            if (!int.TryParse(schoolYearAsObject.ToString(), out schoolYear))
                return;

            // If we're still here, set the context value
            schoolYearContextProvider.SetSchoolYear(schoolYear);
        }
    }
}