using System;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using EdFi.Ods.Api.Common;
using Newtonsoft.Json.Linq;

namespace EdFi.Ods.Api.Services.Binders
{
    public class ResourceAsJsonObjectModelBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var model = (ResourceAsJsonObject<Guid>) Activator.CreateInstance(bindingContext.ModelType);
            bindingContext.Model = model;

            // Deserialize the JSON body, synchronously
            actionContext.ControllerContext.Request.Content.ReadAsStringAsync()
                         .ContinueWith(json => model.Data = JObject.Parse(json.Result),
                                       TaskContinuationOptions.ExecuteSynchronously)
                         .Wait();
            
            return true;
        }
    }
}