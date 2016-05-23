using System.Collections.Generic;
using System.Runtime.Serialization;
using EdFi.Common;
using EdFi.Common.Extensions;
using EdFi.Ods.Common;
using EdFi.Ods.Swagger.Models;
using Newtonsoft.Json;


namespace EdFi.Ods.Swagger
{
    public interface ISwaggerSpec
    {
        ResourceListing GetResourceListing();
        ApiDeclaration GetApiDeclaration(string resourcePath);
    }

    [DataContract]
    public class ResourceListing
    {
        [DataMember(Name = "apiVersion")]
        public string ApiVersion { get; set; }
        [DataMember(Name = "swaggerVersion")]
        public string SwaggerVersion { get; set; }
        [DataMember(Name = "basePath")]
        public string BasePath { get; set; }
        [DataMember(Name = "apis")]
        public ICollection<ApiDeclarationLink> Apis { get; set; }
        [DataMember(Name = "info")]
        public InfoSpec Info { get; set; }
    }

    [DataContract]
    public class ApiDeclarationLink
    {
        [DataMember(Name = "path")]
        public string Path { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
    }

    [DataContract]
    public class ApiDeclaration
    {
        [DataMember(Name = "apiVersion")]
        public string ApiVersion { get; set; }
        [DataMember(Name = "swaggerVersion")]
        public string SwaggerVersion { get; set; }
        [DataMember(Name = "basePath")]
        public string BasePath { get; set; }
        [DataMember(Name = "resourcePath")]
        public string ResourcePath { get; set; }
        [DataMember(Name = "produces")]
        public IList<string> Produces { get; set; }
        [DataMember(Name = "apis")]
        public ICollection<ApiSpec> Apis { get; set; }
        [DataMember(Name = "models")]
        public ModelDictionarySpec Models { get; set; }
    }

    [DataContract]
    public class ApiSpec
    {
        [DataMember(Name = "path")]
        public string Path { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "operations")]
        public ICollection<ApiOperationSpec> Operations { get; set; }
    }

    [DataContract]
    public class ItemsSpec
    {
        [DataMember(Name = "$ref")]
        public string Ref { get; set; }
    }

    [DataContract]
    public class ApiOperationSpec
    {
        [DataMember(Name = "Method")]
        public string Method { get; set; }
        [DataMember(Name = "nickname")]
        public string Nickname { get; set; }
        [DataMember(Name = "notes")]
        public string Notes { get; set; }
        [DataMember(Name = "summary")]
        public string Summary { get; set; }
        [DataMember(Name = "type")]
        public string Type { get; set; }
        [DataMember(Name = "items")]
        public ItemsSpec Items { get; set; }
        [DataMember(Name = "responseClass")]
        public string ResponseClass { get { return Type; } }
        [DataMember(Name = "parameters")]
        public ICollection<ApiParameterSpec> Parameters { get; set; }
        [DataMember(Name = "responseMessages")]
        public ICollection<ApiResponseMessageSpec> ResponseMessages { get; set; }
    }

    [DataContract]
    public class ApiParameterSpec
    {
        private string _type;

        [DataMember(Name = "paramType")]
        public string ParamType { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "type")]
        public string Type {
            get { return _type; }
            set { _type = value.ToCamelCase(); }
        }
        [DataMember(Name = "items")]
        public ItemsSpec Items { get; set; }
        [DataMember(Name = "format")]
        public string Format { get; set; }
        [DataMember(Name = "required")]
        public bool Required { get; set; }
        [DataMember(Name = "allowMultiple")]
        public bool AllowMultiple { get; set; }
        [DataMember(Name = "enum")]
        public IList<string> Enum { get; set; }
        [DataMember(Name = "minimum")]
        public string Minimum { get; set; }
        [DataMember(Name = "maximum")]
        public string Maximum { get; set; }
    }

    [DataContract]
    public class ApiResponseMessageSpec
    {
        [DataMember(Name = "code")]
        public int Code { get; set; }
        [DataMember(Name = "message")]
        public string Message { get; set; }
        [DataMember(Name = "responseModel")]
        public string ResponseModel { get; set; }
    }

    [DataContract]
    [JsonConverter(typeof(CustomConverterForSwaggerModels))]
    public class ModelDictionarySpec : Dictionary<string, ModelSpec>
    {
    }

    [DataContract]
    public class ModelSpec
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }
        [DataMember(Name = "properties")]
        public Dictionary<string, ModelPropertySpec> Properties { get; set; }
    }

    [DataContract]
    public class ModelPropertySpec
    {
        private string _type;

        [DataMember(Name = "type")]
        public string Type {
            get { return _type; }
            set { _type = value.ToCamelCase(); }
        }

        [DataMember(Name = "items")]
        public ItemsSpec Items { get; set; }
       
        [DataMember(Name = "format")]
        public string Format { get; set; }
       
        [DataMember(Name = "required")]
        public bool Required { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "enum")]
        public IList<string> Enum { get; set; }
        
        [DataMember(Name = "minimum")]
        public string Minimum { get; set; }
        
        [DataMember(Name = "maximum")]
        public string Maximum { get; set; }
    }

    [DataContract]
    public class InfoSpec
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "termsOfServiceUrl")]
        public string TermsOfServiceUrl { get; set; }
        [DataMember(Name = "contact")]
        public string Contact { get; set; }
        [DataMember(Name = "license")]
        public string License { get; set; }
        [DataMember(Name = "licenseUrl")]
        public string LicenseUrl { get; set; }
    }
}