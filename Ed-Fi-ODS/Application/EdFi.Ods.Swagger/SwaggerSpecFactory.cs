using System.Web.Http;

namespace EdFi.Ods.Swagger
{
    public class SwaggerSpecFactory
    {
        static SwaggerSpecFactory()
        {
            SwaggerSpecConfig.Instance.PostFilter<ApiOperationPostFilter>();
        }

        public static ISwaggerSpec GetSpec()
        {
            return new ApiExplorerAdapter(
                GlobalConfiguration.Configuration.Services.GetApiExplorer(),
                SwaggerSpecConfig.Instance.BasePathResolver,
                SwaggerSpecConfig.Instance.BaseInfoSpecProvider,
                SwaggerSpecConfig.Instance.PostFilters
                );
        }
    }
}