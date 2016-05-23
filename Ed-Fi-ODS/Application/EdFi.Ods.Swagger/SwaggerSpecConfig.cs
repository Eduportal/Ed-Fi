using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Web;
using System.Web.Http.Description;

namespace EdFi.Ods.Swagger
{
    public interface IOperationSpecFilter
    {
        void Apply(ApiDescription apiDescription, ApiOperationSpec operationSpec);
    }

    public class SwaggerSpecConfig
    {
        internal static readonly SwaggerSpecConfig Instance = new SwaggerSpecConfig();

        public static void Customize(Action<SwaggerSpecConfig> customize)
        {
            customize(Instance);
        }

        private SwaggerSpecConfig()
        {
            PostFilters = new List<IOperationSpecFilter>();
            BasePathResolver = DefaultBasePathResolver;
            BaseInfoSpecProvider = DefaultInfoSpecProvider;
        }

        internal ICollection<IOperationSpecFilter> PostFilters { get; private set; }
        internal Func<string> BasePathResolver { get; private set; }
        internal Func<InfoSpec> BaseInfoSpecProvider { get; private set; }

        public void PostFilter(IOperationSpecFilter operationSpecFilter)
        {
            PostFilters.Add(operationSpecFilter);
        }

        public void PostFilter<TFilter>()
            where TFilter : IOperationSpecFilter, new()
        {
            PostFilters.Add(new TFilter());
        }

        public void ResolveBasePath(Func<string> resolver)
        {
            if(resolver == null)
                throw new ArgumentNullException("resolver");
            BasePathResolver = resolver;
        }

        private string DefaultBasePathResolver()
        {
            //return (HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpRuntime.AppDomainAppVirtualPath).TrimEnd('/');
            return "%BASE_URL%";
        }

        public void ProvideInfoSpec(Func<InfoSpec> provider)
        {
            if(provider == null)
                throw new ArgumentException("provider");
            BaseInfoSpecProvider = provider;
        }

        private InfoSpec DefaultInfoSpecProvider()
        {
            return new InfoSpec
            {
                Title = @"Operational Data Store",
                Description =
                    @"The Ed-Fi ODS API enables applications to read and write education data stored in an Ed-Fi ODS through a secure REST interface. The Ed-Fi ODS API supports both transactional and bulk modes of operation.",
                LicenseUrl = new Url("http://www.ed-fi.org/license/").Value,
                Contact = @"support@ed-fi.org",
                TermsOfServiceUrl = new Url("http://www.ed-fi.org/license/").Value
            };
        }
    }
}