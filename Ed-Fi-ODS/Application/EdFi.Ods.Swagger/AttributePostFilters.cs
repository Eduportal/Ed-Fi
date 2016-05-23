using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Http.Description;

namespace EdFi.Ods.Swagger
{
    public abstract class AttributePostFilters : IOperationSpecFilter
    {
        public abstract void Apply(System.Web.Http.Description.ApiDescription apiDescription,
            ApiOperationSpec operationSpec);
    }

    /// <summary>
    /// Handle the ApiOperation attribute which includes "summary" and "notes" fields
    /// </summary>
    public class ApiOperationPostFilter : AttributePostFilters
    {
        public override void Apply(ApiDescription apiDescription, ApiOperationSpec operationSpec)
        {
            var type = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerType;
            var attr = type.GetCustomAttributes<DescriptionAttribute>().FirstOrDefault();
            if (attr != null)
            {
                
            }
        }
    }
}
