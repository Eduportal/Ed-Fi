using System;
using System.Linq;
using System.Web.Http;
using Castle.Windsor;
using EdFi.Identity.Controllers;
using EdFi.Ods.Api.Services.Controllers;
using EdFi.Ods.Api.Services.Controllers.v2.AcademicSubjectDescriptors;
using EdFi.Ods.Api.Services.Controllers.v2.AcademicSubjectTypes;
using EdFi.Ods.Api.Services.Controllers.v2.AcademicWeeks;
using EdFi.Ods.Api.Services.Metadata;
using EdFi.Ods.Api.Startup;
using EdFi.Ods.Common.Utils.Extensions;
using EdFi.Ods.Swagger;
using EdFi.Ods.Tests.EdFi.Ods.WebApi.Common;
using NUnit.Framework;

namespace EdFi.Ods.Tests.EdFi.Ods.WebApi
{
    public class UrlRoutingTests
    {
        private class TestStartup : StartupBase
        {
            protected override void InstallConfigurationSpecificInstaller(IWindsorContainer container)
            {
                throw new NotImplementedException();
            }

            public void ExternalizeConfigureRoutes(HttpConfiguration config)
            {
                ConfigureRoutes(config);
            }
        }

        private HttpConfiguration _config;
        private RouteTester _routeTester;
        private Action<string> _resultValidator;
        
        [TestFixtureSetUp]
        public void SetUp()
        {
            var startup = new TestStartup();
            _config = new HttpConfiguration {IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always};
            startup.ExternalizeConfigureRoutes(_config);
            _config.EnsureInitialized();
            _routeTester = new RouteTester(_config);
        }

        public class When_Given_A_Url : UrlRoutingTests
        {
            [Test]
            public void Should_Resolve_To_Appropriate_Controller_Action_For_Valid_Urls()
            {
                _resultValidator = x => { if (!string.IsNullOrEmpty(x)) Assert.Fail(x); };
                var validRouteMappingTestData = new[]
                    {
                        //Define Urls to be tested here, along with the expected controller and action they should invoke
                         Tuple.Create("GET","http://localhost/swagger/other/api-docs/xyz/",typeof(SwaggerController),"Get")//Swagger Controller (For static design time file generation)
                        ,Tuple.Create("GET","http://localhost/metadata/other/api-docs/identities/",typeof(MetadataController),"Get")//Swagger resources list
                        ,Tuple.Create("GET","http://localhost/metadata/other/api-docs/",typeof(MetadataController),"Get")//Swagger resource
                        ,Tuple.Create("GET","http://localhost/api/v2.0/2008/academicSubjectDescriptors/1",typeof(AcademicSubjectDescriptorsController),"Get")//Descriptors
                        ,Tuple.Create("GET","http://localhost/api/v2.0/2009/academicWeeks",typeof(AcademicWeeksController),"Get")//Resources
                        ,Tuple.Create("GET","http://localhost/api/v2.0/2010/academicSubjectTypes",typeof(AcademicSubjectTypesController),"Get")//Types
                        ,Tuple.Create("GET","http://localhost/api/v2.0/identities/xyz",typeof(IdentitiesController),"GetById")//Other Section
                        ,Tuple.Create("GET","http://localhost/api/v2.0/2010/bulkoperations/xyz",typeof(BulkOperationsController),"Get")//Other Section
                        ,Tuple.Create("GET","http://localhost/api/v2.0/2010/bulkoperations/67b2bfa3-0dca-48a6-99fb-7531d45a3d88/exceptions/67b2bfa3-0dca-48a6-99fb-7531d45a3d88?offset=1&limit=1",typeof(BulkOperationsExceptionsController),"Get")//Other Section
                        ,Tuple.Create("POST","http://localhost/api/v2.0/2010/uploads/67b2bfa3-0dca-48a6-99fb-7531d45a3d88/commit",typeof(UploadsController),"PostCommit")
                        ,Tuple.Create("POST","http://localhost/api/v2.0/2010/uploads/67b2bfa3-0dca-48a6-99fb-7531d45a3d88/chunk?offset=1&size=1",typeof(UploadsController),"PostChunk")
                    };
                validRouteMappingTestData.Select(x => _routeTester.ValidateRoutingForUrl(x.Item1, x.Item2, x.Item3, x.Item4)).ForEach(_resultValidator);
            }

            [Test]
            public void Should_Not_Resolve_To_Any_Controller_For_Invalid_Urls()
            {
                _resultValidator = x => { if (!x.Contains("failed to resolve to any controller")) Assert.Fail("Invalid Url matched a controller: " + x); };
                //Valid Routes
                var validRouteMappingTestData = new[]
                    {
                        //Define Urls to be tested here, along with the expected controller and action they should invoke
                         Tuple.Create("GET","http://localhost/api/v2.0/academicSubjectDescriptors/1",typeof(AcademicSubjectDescriptorsController),"Get")//Descriptors
                        ,Tuple.Create("GET","http://localhost/api/v2.0/academicWeeks",typeof(AcademicWeeksController),"Get")//Resources
                        ,Tuple.Create("GET","http://localhost/api/v2.0/academicSubjectTypes",typeof(AcademicSubjectTypesController),"Get")//Types
                        ,Tuple.Create("GET","http://localhost/api/v2.0/2001/identities/xyz",typeof(IdentitiesController),"GetById")//Other Section
                        ,Tuple.Create("GET","http://localhost/api/v2.0/bulkoperations/xyz",typeof(BulkOperationsController),"Get")
                        ,Tuple.Create("GET","http://localhost/api/v2.0/bulkoperations/67b2bfa3-0dca-48a6-99fb-7531d45a3d88/exceptions/67b2bfa3-0dca-48a6-99fb-7531d45a3d88?offset=1&limit=1",typeof(BulkOperationsExceptionsController),"Get")
                        ,Tuple.Create("POST","http://localhost/api/v2.0/uploads/67b2bfa3-0dca-48a6-99fb-7531d45a3d88/commit",typeof(UploadsController),"PostCommit")
                        ,Tuple.Create("POST","http://localhost/api/v2.0/uploads/67b2bfa3-0dca-48a6-99fb-7531d45a3d88/chunk?offset=1&size=1",typeof(UploadsController),"PostChunk")
                    };
                validRouteMappingTestData.Select(x => _routeTester.ValidateRoutingForUrl(x.Item1, x.Item2, x.Item3, x.Item4)).ForEach(_resultValidator);
            }
        }
    }
}
