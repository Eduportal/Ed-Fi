using System;
using System.Collections.Generic;
using System.IO;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using EdFi.Ods.BulkLoad.Core;
using EdFi.Ods.BulkLoad.Core.Controllers;
using EdFi.Ods.BulkLoad.Core.Controllers.Aggregates;
using EdFi.Ods.Common;
using EdFi.Ods.CommonUtils;
using EdFi.Ods.CommonUtils.FileSystem;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Entities.Common.Providers;
using EdFi.Ods.Entities.Common.Validation;
using EdFi.Ods.Entities.NHibernate;
using EdFi.Ods.Entities.Repositories.NHibernate;
using EdFi.Ods.Pipelines;
using EdFi.Ods.Pipelines.Factories;
using EdFi.Ods.Pipelines.NHibernate;
using EdFi.Ods.Pipelines.NHibernate.Factories;
using EdFi.Ods.Repositories.NHibernate.Architecture;
using EdFi.Ods.WebApi.Common;
using EdFi.Ods.WebApi.Common.Auth;
using EdFi.Ods.WebApi.Data.EntityFramework.Contexts;
using EdFi.Ods.XmlShredding;
using EdFi.Ods.XmlShredding.CodeGen;
using EdFi.Ods.Common.ExceptionHandling;
using EdFi.Ods.Common.ExceptionHandling.Translators;
using NHibernate;
using NHibernate.Event;
using Configuration = NHibernate.Cfg.Configuration;
using ISession = NHibernate.ISession;

namespace EdFi.Ods.BulkLoad.Console
{
    public class Boostrapper
    {
        public static void Bootstrap(BulkLoaderConfiguration bulkLoaderConfiguration)
        {
            var connectionString = bulkLoaderConfiguration.ConnectionString;
            
            var nHibernateConfiguration = BuildNHibernateConfiguration();
            nHibernateConfiguration.SetProperty(
                NHibernate.Cfg.Environment.ConnectionString, connectionString);

            var connectionStringProvider = new BulkLoadSandboxConnectionStringProvider(connectionString);

            var container =
                new WindsorContainer();

            IoC.Initialize(container);

            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel));
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

            container.Install(FromAssembly.This())
                .Register(Component.For<IProvideSandboxConnectionString>().Instance(connectionStringProvider))
                .Register(Component.For<IBulkExecutor>().ImplementedBy<BulkLoadExecutor>())
                .Register(Component
                    .For<ISessionFactory>()
                    .UsingFactoryMethod(nHibernateConfiguration.BuildSessionFactory)
                    .LifeStyle.Singleton)
                .Register(Component
                    .For<ISession>()
                    .UsingFactoryMethod(c => c.Resolve<ISessionFactory>().OpenSession())
                    .LifeStyle.PerThread)
                .Register(Component
                    .For<ISourceFiles, ISourceLocalOnlyFiles>()
                    .ImplementedBy<SourceFileLocator>())
                .Register(Component
                    .For<IObjectValidator>()
                    .ImplementedBy<DataAnnotationsObjectValidator>())
                .Register(Component
                    .For<IObjectValidator>()
                    .ImplementedBy<UniqueIdNotChangedObjectValidator>())
                .Register(Classes
                    .FromAssemblyContaining<Marker_EdFi_Ods_Repositories_NHibernate>()
                    .BasedOn(typeof (IRepository<,>))
                    .WithService.AllInterfaces())
                .Register(Component
                    .For<IETagProvider>()
                    .ImplementedBy<ETagProvider>())
                .Register(Classes.FromAssemblyContaining<Marker_EdFi_Ods_XmlShredding>()
                    .BasedOn(typeof (IResourceFactory<>))
                    .WithService.AllInterfaces())
                .Register(Classes.FromAssemblyContaining<ILoadAggregates>()
                    .BasedOn(typeof (ILoadAggregates))
                    .WithService.AllInterfaces())
                .Register(Classes.FromAssemblyContaining<Marker_EdFi_Ods_Pipeline>()
                    .BasedOn(typeof (ICreateOrUpdatePipeline<,>))
                    .WithService.AllInterfaces())
                .Register(Component
                    .For<IControlBulkLoading>()
                    .ImplementedBy<BulkLoadMaster>())
                .Register(Component
                    .For<IBulkLoader>()
                    .ImplementedBy<BulkLoader>())
                .Register(Component
                    .For<IBulkOperationDbContext>()
                    .ImplementedBy<BulkOperationDbContext>()
                    .LifestyleTransient())
                .Register(Component
                    .For(typeof (IDbExecutor<>))
                    .ImplementedBy(typeof (DbExecutor<>)))
                .Register(Component
                    .For(typeof (IExceptionTranslator))
                    .ImplementedBy(typeof (SqlServerConstraintExceptionTranslator)))
                .Register(Component
                    .For<IXPathMapBuilder>()
                    .ImplementedBy<XPathMapBuilder>())
                .Register(Component
                    .For<IDatabaseMetadataProvider>()
                    .ImplementedBy<DatabaseMetadataProvider>())
                .Register(Component
                    .For<IWebServiceErrorProvider>()
                    .ImplementedBy<WebServiceErrorProvider>())
                .Register(Component
                    .For<IOutput>()
                    .ImplementedBy<ConsoleOutput>())
                .Register(Component
                    .For<IServiceLocator>()
                    .ImplementedBy<CastleWindsorServiceLocator>())
                .Register(Types
                    .FromAssemblyContaining<Marker_EdFi_Ods_Common_ExceptionHandling_Translators>()
                    .BasedOn<IExceptionTranslator>()
                    .WithService.FromInterface())
                .Register(Component
                    .For<Func<string, string, IXmlGPS>>()
                    .UsingFactoryMethod<Func<string, string, IXmlGPS>>(
                        () => (x, y) => new XmlGPS(IoC.Resolve<IXPathMapBuilder>().DeserializeMap(x), y)))
                .Register(Component
                    .For<Func<string, IEnumerable<string>, IIndexedXmlFileReader>>()
                    .UsingFactoryMethod<Func<string, IEnumerable<string>, IIndexedXmlFileReader>>(
                        () =>
                            (x, y) =>
                                new IndexedXmlFileReader(x, new FileStreamBuilder(),
                                    IoC.Resolve<Func<string, string, IXmlGPS>>(), y))
                )
                .Register(Component
                    .For<IPipelineFactory>()
                    .ImplementedBy<ResourceToSqlServerPipelineFactory>())
                .Register(
                    Component.For<IHaveInterchangeSpecificLoaderCollections>()
                        .ImplementedBy<GeneratedInterchangeSpecificLoaderCollections>())
                .Register(Component
                    .For<IDictionary<string, IInterchangeController>>()
                    .UsingFactoryMethod(BuildSequencedInterchangeControllerDictionary));

            container.Register(Component.For<IPersonUniqueIdProvider>().ImplementedBy<PersonUniqueIdProvider>());
            PersonIdUsiCache.GetCache = () => PersonIdUsiCache.InternalUseInstance;

            TypesAndDescriptorsCache.GetCache = () => TypesAndDescriptorsCache.InternalUseInstance;

            var pipelineStepAssemblyMarkerTypes = new[]
            {
                // List of assemblies to search
                typeof (Marker_EdFi_Ods_Pipeline),
                typeof (Marker_EdFi_Ods_Pipelines_NHibernate)
            };

            // Register the steps in each pipeline step assembly
            foreach (var markerType in pipelineStepAssemblyMarkerTypes)
            {
                container.Register(Classes
                    .FromAssemblyContaining(markerType)
                    .BasedOn(typeof (IStep<,>)));
            }
        }

        // ------------------------------------- IMPORTANT ---------------------------------------------------//
        //  This method is what controls the order in which Interchange files will be loaded.  As of EdFI 2.0
        //  the expected order in encapsulated in the SequencedInterchangeComparer class
        // ------------------------------------- IMPORTANT ---------------------------------------------------//
        private static IDictionary<string, IInterchangeController> BuildSequencedInterchangeControllerDictionary()
        {
            var loaders = IoC.Resolve<IHaveInterchangeSpecificLoaderCollections>();
            var fileReaderFunc = IoC.Resolve<Func<string, IEnumerable<string>, IIndexedXmlFileReader>>();
            var collection = new SortedDictionary<string, IInterchangeController>(new SequencedInterchangeComparer());
            collection.Add(InterchangeTypes.Descriptors, new DescriptorInterchangeController(loaders.GetCollectionFor(InterchangeTypes.Descriptors), fileReaderFunc));
            collection.Add(InterchangeTypes.Standards, new StandardsInterchangeController(loaders.GetCollectionFor(InterchangeTypes.Standards), fileReaderFunc));
            collection.Add(InterchangeTypes.EducationOrganization, new EducationOrganizationInterchangeController(loaders.GetCollectionFor(InterchangeTypes.EducationOrganization), fileReaderFunc));
            collection.Add(InterchangeTypes.EducationOrganizationCalendar, new EducationOrganizationCalendarInterchangeController(loaders.GetCollectionFor(InterchangeTypes.EducationOrganizationCalendar), fileReaderFunc));
            collection.Add(InterchangeTypes.StudentEnrollment, new StudentEnrollmentInterchangeController(loaders.GetCollectionFor(InterchangeTypes.StudentEnrollment), fileReaderFunc));
            collection.Add(InterchangeTypes.Staff, new StaffInterchangeController(loaders.GetCollectionFor(InterchangeTypes.Staff), fileReaderFunc));
            collection.Add(InterchangeTypes.StudentParent, new StudentParentInterchangeController(loaders.GetCollectionFor(InterchangeTypes.StudentParent), fileReaderFunc));
            collection.Add(InterchangeTypes.MasterSchedule, new MasterScheduleInterchangeController(loaders.GetCollectionFor(InterchangeTypes.MasterSchedule), fileReaderFunc));
            collection.Add(InterchangeTypes.StudentProgram, new StudentProgramInterchangeController(loaders.GetCollectionFor(InterchangeTypes.StudentProgram), fileReaderFunc));
            collection.Add(InterchangeTypes.StudentCohort, new StudentCohortInterchangeController(loaders.GetCollectionFor(InterchangeTypes.StudentCohort), fileReaderFunc));
            collection.Add(InterchangeTypes.StudentGradebook, new StudentGradebookInterchangeController(loaders.GetCollectionFor(InterchangeTypes.StudentGradebook), fileReaderFunc));
            collection.Add(InterchangeTypes.StudentGrade, new StudentGradeInterchangeController(loaders.GetCollectionFor(InterchangeTypes.StudentGrade), fileReaderFunc));
            collection.Add(InterchangeTypes.StudentAttendance, new StudentAttendenceInterchangeController(loaders.GetCollectionFor(InterchangeTypes.StudentAttendance), fileReaderFunc));
            collection.Add(InterchangeTypes.AssessmentMetadata, new AssessmentMetadataController(loaders.GetCollectionFor(InterchangeTypes.AssessmentMetadata), fileReaderFunc));
            collection.Add(InterchangeTypes.StudentAssessment, new StudentAssessmentController(loaders.GetCollectionFor(InterchangeTypes.StudentAssessment), fileReaderFunc));
            collection.Add(InterchangeTypes.StudentDiscipline, new StudentDisciplineController(loaders.GetCollectionFor(InterchangeTypes.StudentDiscipline), fileReaderFunc));
            collection.Add(InterchangeTypes.StudentIntervention, new StudentInterventionController(loaders.GetCollectionFor(InterchangeTypes.StudentIntervention), fileReaderFunc));
            collection.Add(InterchangeTypes.PostSecondary, new PostSecondaryController(loaders.GetCollectionFor(InterchangeTypes.PostSecondary), fileReaderFunc));
            collection.Add(InterchangeTypes.StudentTranscript, new StudentTranscriptController(loaders.GetCollectionFor(InterchangeTypes.StudentTranscript), fileReaderFunc));
            collection.Add(InterchangeTypes.Finance, new FinanceInterchangeController(loaders.GetCollectionFor(InterchangeTypes.Finance), fileReaderFunc));
            return collection;
        } 

        private static Configuration BuildNHibernateConfiguration()
        {
            var configuration = new Configuration().Configure();
            configuration.Interceptor = new CreateDateBasedTransientInterceptor();
            configuration.SetListener(ListenerType.PreInsert, new CreateDatePreInsertListener());
            configuration.SetListener(ListenerType.PostInsert, new CreateDatePostInsertListener());
            return configuration;
        }
    }
}
