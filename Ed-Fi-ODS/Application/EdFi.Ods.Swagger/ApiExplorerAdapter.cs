using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using EdFi.Common;
using EdFi.Common.Extensions;
using EdFi.Ods.Common;
using EdFi.Ods.Swagger.Attributes;
using EdFi.Ods.Api.Common;

namespace EdFi.Ods.Swagger
{
    public class ApiExplorerAdapter : ISwaggerSpec
    {
        protected const string SwaggerVersion = "1.2";
        private readonly Lazy<Dictionary<string, ApiDeclaration>> _apiDeclarations;
        private readonly Func<string> _basePathAccessor;
        private readonly Func<InfoSpec> _infoSpecProvider;
        private readonly IEnumerable<IGrouping<string, ApiDescription>> _controllerGroups;
        private readonly IEnumerable<IOperationSpecFilter> _postFilters;
        private readonly Lazy<ResourceListing> _resourceListing;
        

        public ApiExplorerAdapter(
            IApiExplorer apiExplorer,
            Func<string> basePathAccessor,
            Func<InfoSpec> infoSpecProvider,
            IEnumerable<IOperationSpecFilter> postFilters)
        {
            // Group ApiDescriptions by controller name - each group corresponds to an ApiDeclaration
            _controllerGroups =
                from ad in apiExplorer.ApiDescriptions
                let controllerName = ad.ActionDescriptor.ControllerDescriptor.ControllerType.Name
                where controllerName != "SwaggerController"
                      && controllerName != "MetadataController"
                group ad by "/" + controllerName.ToLower() into g
                orderby g.Key
                select g;

            _basePathAccessor = basePathAccessor;
            _infoSpecProvider = infoSpecProvider;
            _postFilters = postFilters;
            _resourceListing = new Lazy<ResourceListing>(GenerateResourceListing);
            _apiDeclarations = new Lazy<Dictionary<string, ApiDeclaration>>(GenerateApiDeclarations);
        }

        public ResourceListing GetResourceListing()
        {
            return _resourceListing.Value;
        }

        public ApiDeclaration GetApiDeclaration(string resourcePath)
        {
            var dic = _apiDeclarations.Value;

            if(!dic.ContainsKey(resourcePath.ToLower()))
                throw new ArgumentException("resourcePath");

            return dic[resourcePath.ToLower()];
        }

        private ResourceListing GenerateResourceListing()
        {
            var declarationLinks = _controllerGroups
                .Select(dg => new ApiDeclarationLink
                {
                    Path = dg.Key.ToCamelCase(),
                    Description = dg.Description()
                })
                .Where(x=>!x.Path.ToLower().Equals("/metadata"))
                .ToList();

            var version = GetType().Assembly.GetName().Version.ToString();

            return new ResourceListing
                {
                    ApiVersion = version,
                    SwaggerVersion = SwaggerVersion,
                    BasePath = _basePathAccessor() + "/metadata/other/api-docs",
                    Apis = declarationLinks,
                    Info = _infoSpecProvider()
                };
        }

        //Should be private, but made public to unit test and avoid a truck load of mock objects
        public Dictionary<string, ApiDeclaration> GenerateApiDeclarations()
        {
            return _controllerGroups
                .ToDictionary(cg => cg.Key, DescriptionGroupToApiDeclaration);
        }

        private T GetAttribute<T>(ApiDescription ad) where T : Attribute
        {
            var attribute = ad.ActionDescriptor.GetCustomAttributes<T>().FirstOrDefault();
            if (attribute == null) throw new Exception(typeof(T).Name + " attribute not found on Method " + ad.ActionDescriptor.ActionName + " for " + ad.ID);
            return attribute;
        }

        private T GetAttribute<T>(HttpControllerDescriptor cd) where T : Attribute
        {
            var attribute = cd.GetCustomAttributes<T>().FirstOrDefault();
            if (attribute == null) throw new Exception(typeof(T).Name + " attribute not found on Method " + cd.ControllerName);
            return attribute;
        }

        private string GetRoute(ApiDescription ad)
        {
            var apiRequestAttribute = GetAttribute<ApiRequestAttribute>(ad);
            if (apiRequestAttribute.Route == null) throw new Exception("ApiRequestAttribute Route Property not found on Method " + ad.ActionDescriptor.ActionName + " for " + ad.ID);
            return apiRequestAttribute.Route;
        }

        private Type GetResponseClass(HttpActionDescriptor ad)
        {
            var attribute = ad.GetCustomAttributes<ApiRequestAttribute>().FirstOrDefault();
            if (attribute != null)
            {
                var returnType = attribute.Type;
                if (returnType != null)
                {
                    return returnType;
                }
            }
            return typeof(void);
        }

        private ApiDeclaration DescriptionGroupToApiDeclaration(IGrouping<string, ApiDescription> descriptionGroup)
        {
            var modelSpecsBuilder = new ModelSpecsBuilder();

            // Group further by relative path - each group corresponds to an ApiSpec
            var apiSpecs = descriptionGroup
                .GroupBy(GetRoute)
                .Select(dg => DescriptionGroupToApiSpec(dg, modelSpecsBuilder))
                .ToList();

            var controllerType = descriptionGroup.First().ActionDescriptor.ControllerDescriptor.ControllerType;



            var version = controllerType.Assembly.GetName().Version.ToString();

            var basePath = _basePathAccessor() + "/api/v2.0";

            return new ApiDeclaration
                {
                    ApiVersion = version,
                    SwaggerVersion = SwaggerVersion,
                    BasePath =  basePath,
                    ResourcePath = descriptionGroup.Key,
                    Produces = new[] { "application/json" },
                    Apis = apiSpecs,
                    Models = modelSpecsBuilder.Build()
                };
        }

        private ApiSpec DescriptionGroupToApiSpec(IGrouping<string, ApiDescription> descriptionGroup, ModelSpecsBuilder modelSpecsBuilder)
        {
            var pathParts = descriptionGroup.Key.Split('?');
            var queryString = pathParts.Length == 1 ? String.Empty : pathParts[1];

            var operationSpecs = descriptionGroup
                .Select(dg => DescriptionToOperationSpec(dg, queryString, modelSpecsBuilder))
                .ToList();

            var description = GetAttribute<DescriptionAttribute>(descriptionGroup.First().ActionDescriptor.ControllerDescriptor).Description;

            return new ApiSpec
                {
                    Path = descriptionGroup.Key,
                    Description = description,
                    Operations = operationSpecs
                };
        }

        private ApiOperationSpec DescriptionToOperationSpec(ApiDescription description, string queryString, ModelSpecsBuilder modelSpecsBuilder)
        {
            var paramSpecs = description.ParameterDescriptions
                .Where(x => x.ParameterDescriptor != null)
                .SelectMany(pd => ParamDescriptionToParameterSpec(pd, modelSpecsBuilder))
                .ToList();

            var classType = GetResponseClass(description.ActionDescriptor);
            TypeCategory category;
            Type containedType;
            var type = classType.ToSwaggerType(out category, out containedType);
            ItemsSpec items = null;
            modelSpecsBuilder.AddType(classType);
            if (containedType != null)
            {
                items = new ItemsSpec() { Ref = containedType.ToSwaggerType() };
            }
            

            var operationSpec = new ApiOperationSpec
                {
                    Method = description.HttpMethod.Method,
                    Nickname = description.ActionDescriptor.ActionName,
                    Parameters = paramSpecs,
                    Type = type,
                    Items = items,
                    ResponseMessages = description.ResponseMessageSpecs(),
                    Summary = description.Summary() ?? description.Documentation,
                    Notes = description.Notes()
                };

            foreach (var filter in _postFilters)
            {
                filter.Apply(description, operationSpec);
            }

            return operationSpec;
        }

        private IEnumerable<ApiParameterSpec> ParamDescriptionToParameterSpec(ApiParameterDescription parameterDescription, ModelSpecsBuilder modelSpecsBuilder)
        {
            var attribute = parameterDescription.ApiMemberAttribute();
            if (attribute==null) throw new Exception(parameterDescription.ParameterDescriptor.ParameterName + " in " + parameterDescription.ParameterDescriptor.ActionDescriptor.ActionName + " does not have ApiMember attribute.");
            var paramSource = "";
            switch (parameterDescription.Source)
            {
                case ApiParameterSource.FromBody:
                    paramSource = "body";
                    break;
                case ApiParameterSource.FromUri:
                    paramSource = attribute.ParameterType;
                    break;
                case ApiParameterSource.Unknown:
                    switch (parameterDescription.ETag())
                    {
                        case ETagMatch.IfMatch:
                            parameterDescription.Name = "If-Match";
                            paramSource = "header";
                            break;
                        case ETagMatch.IfNoneMatch:
                            parameterDescription.Name = "If-None-Match";
                            paramSource = "header";
                            break;
                    }
                    break;
            }
            var list = new List<ApiParameterSpec>();

            var paramType = parameterDescription.ParameterDescriptor.ParameterType;
            modelSpecsBuilder.AddType(paramType);
            if (attribute.Expand)
            {
                foreach (var subParamType in paramType.GetProperties().Where(x=>x.PropertyType.IsPublic))
                {
                    list.Add(createApiParameterSpec(subParamType.Name, paramSource, parameterDescription, subParamType.PropertyType,false));
                }
            }
            else
            {
                list.Add(createApiParameterSpec(parameterDescription.Name, paramSource, parameterDescription, paramType,
                    !parameterDescription.ParameterDescriptor.IsOptional));   
            }
            return list.ToArray();
        }

        private ApiParameterSpec createApiParameterSpec(string name, string paramSource, ApiParameterDescription parameterDescription, 
            Type paramType, bool isOptional)
        {
            TypeCategory category;
            Type containedType;
            var type = paramType.ToSwaggerType(out category, out containedType);
            ItemsSpec items = null;
            if (containedType != null)
            {
                items = new ItemsSpec() { Ref = containedType.ToSwaggerType() };
            }
            var nameValue = parameterDescription.ApiMemberAttribute().Name;
            //Use reflection based membername instead of member name specified in attribute because attribute will be same for each property in an expanded complex object
            //Second case is when attribute name is null, then use reflection based one
            if (string.IsNullOrEmpty(nameValue) || parameterDescription.ApiMemberAttribute().Expand)
            {
                nameValue = name;
            }
            var apiParameterSpec = new ApiParameterSpec
            {
                ParamType = paramSource,
                Name = nameValue,
                Description = parameterDescription.ApiMemberAttribute().Description,
                Type = type,
                Items = items,
                Format = paramType.ToSwaggerFormat(),
                Required = isOptional,
                Enum = parameterDescription.ParameterDescriptor.ParameterType.EnumValues(),
            };
            return apiParameterSpec;
        }
    }
}