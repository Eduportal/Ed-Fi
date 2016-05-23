using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common._Installers;
using EdFi.Common.Caching;
using EdFi.Common.Configuration;
using EdFi.Common.Context;
using EdFi.Common.Database;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Messaging;
using EdFi.Common.Security.Claims;
using EdFi.Common.Services;
using EdFi.Messaging.MSMQ._Installers;
using EdFi.Ods.Api.Common.Authorization;
using EdFi.Ods.Api.Data._Installers;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.BulkLoad.Core._Installers;
using EdFi.Ods.BulkLoad.Core.Controllers;
using EdFi.Ods.BulkLoad.Core.ServiceHosts;
using EdFi.Ods.BulkLoad.Services._Installers;
using EdFi.Ods.CodeGen.XmlShredding._Installers;
using EdFi.Ods.Common._Installers;
using EdFi.Ods.Common._Installers.ComponentNaming;
using EdFi.Ods.Common.Database;
using EdFi.Ods.Common.ExceptionHandling._Installers;
using EdFi.Ods.Common.Utils.Extensions;
using EdFi.Ods.Entities.Common._Installers;
using EdFi.Ods.Entities.Repositories.NHibernate._Installers;
using EdFi.Ods.Messaging._Installers;
using EdFi.Ods.Messaging.BulkLoadCommands;
using EdFi.Ods.Pipelines._Installers;
using EdFi.Ods.Security.Metadata.Repositories;
using EdFi.Ods.Security._Installers;
using EdFi.Ods.Security.Claims;
using EdFi.Ods.Security.Metadata.Contexts;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.XmlShredding._Installers;
using Should;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using EdFi.Ods.Api.Pipelines._Installers;

namespace EdFi.Ods.WebService.Tests.Owin
{
    internal class OwinBulkHelper
    {
        internal class TestData
        {
            public string Xml;
            public string Description;
        }

        internal static TestData BuildTestData(string xmlData, string uniqueIdentifier = "")
        {
            var addedUniqueInfo = String.IsNullOrEmpty(uniqueIdentifier)
                ? String.Empty
                : String.Format("{0} - ", uniqueIdentifier);

            var uniqueDescription = String.Format("{0}[{1:MMMM dd, yyyy H:mm:ss}]", addedUniqueInfo, DateTime.Now);
            if (uniqueDescription.Length > 75)
                throw new Exception("Max length for ShortDescription is 75 characters");
            var xml = xmlData.Replace("##SomeUniqueInformation##", uniqueDescription);
            return new TestData { Description = uniqueDescription, Xml = xml };
        }

        internal static readonly IEnumerable<BulkOperationStatus> EndStates = new List<BulkOperationStatus>
        {
            BulkOperationStatus.Completed,
            BulkOperationStatus.Error,
            BulkOperationStatus.Expired
        };

        internal static BulkOperationStatus WaitForBulkOperationToComplete(HttpClient client, string bulkOperationId, string year = "2014")
        {
            var statusUrl = OwinUriHelper.BuildApiUri(year, "BulkOperations" + "/" + bulkOperationId);

            bool isDone;
            var numberOfPolls = 0;

            BulkOperationStatus status;
            do
            {
                Thread.Sleep(1000);
                numberOfPolls++;

                var getResponse = client.GetAsync(statusUrl).Result;
                getResponse.EnsureSuccessStatusCode();
                getResponse.StatusCode.ShouldEqual(HttpStatusCode.OK);

                var content = getResponse.Content.ReadAsAsync<BulkOperationResource>().Result;
                status = content.Status.ToEnum<BulkOperationStatus>();

                isDone = EndStates.Contains(status);

                if (numberOfPolls > 10)
                    throw new TimeoutException();

            } while (!isDone);

            return status;
        }

        internal static IWindsorContainer ConfigureIoCForServices(Func<IDatabaseNameProvider> createDatabaseNameProvider, Func<IOAuthTokenValidator> createOAuthTokenValidator, Func<OwinSecurityRepository> securityRepository, bool yearSpecific = false)
        {
            var containerFactory = new InversionOfControlContainerFactory();

            // Get the container
            var container = containerFactory.CreateContainer(c =>
            {
                c.AddFacility<TypedFactoryFacility>();
                c.AddFacility<DatabaseConnectionStringProviderFacility>();
            }, Assembly.GetExecutingAssembly());

            container.AddSupportForEmptyCollections();

            container.Install(new SecurityComponentsInstaller());

            // Register components for establishing claims from context
            container.Register(Component.For<IClaimsIdentityProvider>().ImplementedBy<ClaimsIdentityProvider>());
            ClaimsPrincipal.ClaimsPrincipalSelector = () => EdFiClaimsPrincipalSelector.GetClaimsPrincipal(container.Resolve<IClaimsIdentityProvider>());

            container.Register(Component.For<IMessageHandler<StartOperationCommand>>().ImplementedBy<BulkLoadMaster>().LifestyleSingleton());
            container.Register(Component.For<IContextStorage>().ImplementedBy<CallContextStorage>());
            container.Register(Component.For<ICacheProvider>().ImplementedBy<MemoryCacheProvider>());
            container.Register(Component.For<IMessageHandler<CommitUploadCommand>>().ImplementedBy<UploadFileCommitHandler>().LifestyleSingleton());
            container.Register(Component.For<IConfigValueProvider>().ImplementedBy<AppConfigValueProvider>());
            container.Register(Component.For<IConfigSectionProvider>().ImplementedBy<AppConfigSectionProvider>());

            container.Install(new BulkLoadCoreInstaller());
            container.Install(new EdFiCommonInstaller());
            container.Install(new EdFiOdsCommonInstaller());
            container.Install(new EdFiOdsCommonExceptionHandlingInstaller());
            container.Install(new EdFiOdsEntitiesCommonInstaller());
            container.Install(new EdFiMessagingInstaller());
            container.Install(new EdFiOdsMessagingInstaller());
            container.Install(new EdFiOdsPipelinesInstaller());
            container.Install(new EdFiOdsRepositoriesNHibernateInstaller());
            container.Install(new EdFiOdsXmlShreddingInstaller());
            container.Install(new EdFiOdsWebApiDataEntityFrameworkInstaller());
            container.Install(new XmlShreddingDatabaseInstaller());
            container.Install(new EdFiOdsXmlShreddingCodeGenInstaller());
            container.Install(new MSMQInstaller());
            container.Install(new LocalFileSystemManagementInstaller());

            container.Register(Component.For<IHostedService>().Named("BulkWorker").ImplementedBy<WindowsBulkWorkerServiceHost>());
            container.Register(Component.For<IHostedService>().Named("UploadWorker").ImplementedBy<WindowsCommitUploadServiceHost>());

            container.Register(Component.For<IConfigConnectionStringsProvider>().ImplementedBy<AppConfigConnectionStringsProvider>());
            container.Register(Component.For<IDatabaseConnectionStringProvider>()
                                    .Named("IDatabaseConnectionStringProvider.Admin")
                                    .ImplementedBy<NamedDatabaseConnectionStringProvider>()
                                    .DependsOn(Dependency.OnValue("connectionStringName", "EdFi_Admin")));

            container.Register(Component.For<IDatabaseConnectionStringProvider>()
                                        .Named("IDatabaseConnectionStringProvider.BulkOperations")
                                        .ImplementedBy<NamedDatabaseConnectionStringProvider>()
                                        .DependsOn(Dependency.OnValue("connectionStringName", "BulkOperationDbContext")));

            container.Register(Component.For<IDatabaseConnectionStringProvider>()
                                        .Named("IDatabaseConnectionStringProvider.EduId")
                                        .ImplementedBy<NamedDatabaseConnectionStringProvider>()
                                        .DependsOn(Dependency.OnValue("connectionStringName", "EduIdContext")));

            container.Register(Component.For<IDatabaseConnectionStringProvider>()
                                        .Named("IDatabaseConnectionStringProvider.Master")
                                        .ImplementedBy<NamedDatabaseConnectionStringProvider>()
                                        .DependsOn(Dependency.OnValue("connectionStringName", "EdFi_master")));

            container.Register(Component.For<IDatabaseConnectionStringProvider>()
                                        .Named("IDatabaseConnectionStringProvider.UniqueIdIntegration")
                                        .ImplementedBy<NamedDatabaseConnectionStringProvider>()
                                        .DependsOn(Dependency.OnValue("connectionStringName", "UniqueIdIntegrationContext")));

            if (!yearSpecific)
            {
                container.Register(Component.For<IDatabaseConnectionStringProvider>()
                                            .Named("IDatabaseConnectionStringProvider.Ods")
                                            .ImplementedBy<PrototypeWithDatabaseNameOverrideDatabaseConnectionStringProvider>()
                                            .DependsOn(Dependency.OnValue("prototypeConnectionStringName", "EdFi_Ods"))
                                            .DependsOn(Dependency.OnComponent(typeof (IDatabaseNameProvider), DatabaseNameStrategyRegistrationKeys.Sandbox)));

                container.Register(Component.For<IDatabaseNameProvider>().Instance(createDatabaseNameProvider()).IsDefault().Named(DatabaseNameStrategyRegistrationKeys.Sandbox));
            }
            else
            {
                container.Register(Component.For<IDatabaseConnectionStringProvider>()
                                            .Named("IDatabaseConnectionStringProvider.Ods")
                                            .ImplementedBy<PrototypeWithDatabaseNameOverrideDatabaseConnectionStringProvider>()
                                            .DependsOn(Dependency.OnValue("prototypeConnectionStringName", "EdFi_Ods"))
                                            .DependsOn(Dependency.OnComponent(typeof(IDatabaseNameProvider), DatabaseNameStrategyRegistrationKeys.YearSpecificOds)));

                container.Register(Component.For<IDatabaseNameProvider>().Instance(createDatabaseNameProvider()).IsDefault().Named(DatabaseNameStrategyRegistrationKeys.YearSpecificOds));
            }

            //overrides
            
            container.Register(Component.For<IOAuthTokenValidator>().Instance(createOAuthTokenValidator()).IsDefault());
            container.Register(Component.For<ISecurityRepository>().Instance(securityRepository()).IsDefault());

            return container;
        }
    }
}
