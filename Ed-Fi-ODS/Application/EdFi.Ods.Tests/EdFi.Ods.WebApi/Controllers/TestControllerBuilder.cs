namespace EdFi.Ods.Tests.EdFi.Ods.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Web.Http;

    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    using global::EdFi.Common.InversionOfControl;
    using global::EdFi.Ods.Api.Common;
    using global::EdFi.Ods.Common.Context;
    using global::EdFi.Ods.Common.ExceptionHandling;
    using global::EdFi.Ods.Common.ExceptionHandling.Translators;
    using global::EdFi.Ods.Pipelines.Common;
    using global::EdFi.Ods.Pipelines.Factories;

    using Rhino.Mocks;

    public class StubEtagProviderSinceWeReallyDontCareWhatTheValueIs : IETagProvider
    {
        public string GetETag(object value)
        {
            return "new" + value;
        }

        public DateTime GetDateTime(string etag)
        {
            return new DateTime(2000, 1, 1);
        }
    }

    public class StubCurrentSchoolYearContextProvider : ISchoolYearContextProvider
    {
        public int GetSchoolYear()
        {
            return DateTime.Now.Year;
        }

        public void SetSchoolYear(int schoolYear)
        {
            throw new NotImplementedException();
        }
    }

    class StubDatabaseMetadataProvider : IDatabaseMetadataProvider
    {
        public IndexDetails GetIndexDetails(string indexName)
        {
            return new IndexDetails
            {
                IndexName = "FK_TableName_IndexId",
                TableName = "TableName",
                ColumnNames = new List<string> { "TableNameId" }
            };
        }
    }

    public static class TestControllerBuilder
    {
        public static T GetController<T>(IPipelineFactory factory, string id = null) where T : ApiController
        {
            var translators = new IExceptionTranslator[]
            {
                new BadRequestExceptionTranslator(),
                new SqlServerConstraintExceptionTranslator(),
                new SqlServerUniqueIndexExceptionTranslator(new StubDatabaseMetadataProvider()),
                new EdFiSecurityExceptionTranslator(),
                new NotFoundExceptionTranslator(), 
                new NotModifiedExceptionTranslator(),
                new ConcurencyExceptionTranslator(),
                new DuplicateNaturalKeyExceptionTranslator(),
                new DuplicateNaturalKeyCreateExceptionTranslator(),
           };

            var schoolYearContextProvider = MockRepository.GenerateStub<ISchoolYearContextProvider>();
            schoolYearContextProvider.Stub(x => x.GetSchoolYear()).Return(DateTime.Now.Year);
            var controller =
                (T)
                    Activator.CreateInstance(typeof (T),
                        new object[] { factory, new StubCurrentSchoolYearContextProvider(), new RESTErrorProvider(translators) });
            controller.Configuration = new HttpConfiguration();
            var uri = string.Format(@"http://localhost/api/v2.0/{0}/Students/{1}",
                schoolYearContextProvider.GetSchoolYear(), id);
            controller.Request = new HttpRequestMessage { RequestUri = new Uri(uri) };
            return controller;
        }

        public static WindsorContainerEx GetWindsorContainer() 
        {
            var container = new WindsorContainerEx();
            container.AddSupportForEmptyCollections();
            container.Register(Component
                .For<ISchoolYearContextProvider>()
                .ImplementedBy<StubCurrentSchoolYearContextProvider>());
            container.Register(Component
                .For<IETagProvider>()
                .ImplementedBy<StubEtagProviderSinceWeReallyDontCareWhatTheValueIs>());
            container.Register(Classes.FromThisAssembly().BasedOn(typeof (IStep<,>)));
            return container;
        }
    }
}