using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EdFi.Ods.BulkLoad.Core.Controllers;
using EdFi.Ods.BulkLoad.Core.Controllers.Aggregates;
using EdFi.Ods.BulkLoad.Core.Controllers.Base;
using EdFi.Ods.BulkLoad.Core.Data;
using EdFi.Ods.BulkLoad.Core.Dictionaries;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Utils;
using EdFi.Ods.CodeGen.XmlShredding;
using FluentValidation;

namespace EdFi.Ods.BulkLoad.Core._Installers
{
    using EdFi.Ods.Api.Data.Model;

    public class BulkLoadCoreInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component
                .For<IDbDictionary>()
                .ImplementedBy<InMemoryDbDictionary>()
                .LifestyleTransient());

            container.Register(Component
                .For<IDirectoryLocator>()
                .ImplementedBy<DirectoryLocator>());

            container.Register(Component
                                   .For<IHaveInterchangeSpecificLoaderCollections>()
                                   .ImplementedBy<GeneratedInterchangeSpecificLoaderCollections>());
            container.Register(Component
                                   .For<Func<string, IIndexedXmlFileReader>>()
                                   .UsingFactoryMethod<Func<string, IIndexedXmlFileReader>>(
                                       () =>
                                       (x) =>
                                       new IndexedXmlFileReader(x, new FileStreamBuilder(),
                                                                container.Resolve<Func<string, string, IXmlGPS>>(), container.Resolve<IDbDictionary>())));
            container.Register(Component
                                   .For<IDictionary<InterchangeType, IInterchangeController>>()
                                   .UsingFactoryMethod(() =>
                                   {
                                       var loaders = container.Resolve<IHaveInterchangeSpecificLoaderCollections>();
                                       var fileReaderFunc = container.Resolve<Func<string, IIndexedXmlFileReader>>();
                                       var collection = new SortedDictionary<InterchangeType, IInterchangeController>(new SequencedInterchangeComparer());
                                       foreach (var interchangeType in InterchangeType.RequiredLoadOrder)
                                       {
                                           collection.Add(interchangeType, new InterchangeController(loaders.GetCollectionFor(interchangeType.Name), fileReaderFunc));
                                       }
                                       return collection;
                                   }),
                               Classes.FromAssemblyContaining<ILoadAggregates>()
                                      .BasedOn(typeof(ILoadAggregates))
                                      .WithService.AllInterfaces());
            container.Register(Component
                                   .For<IControlBulkLoading>()
                                   .ImplementedBy<BulkLoadMaster>().Named("ThisHasANameToDifferentiateFromTheIMessageHandlerInterface"));

            container.Register(Component.For<IOutput>().ImplementedBy<ConsoleOutput>());
            container.Register(Component
                                   .For<Func<string, string, IXmlGPS>>()
                                   .UsingFactoryMethod<Func<string, string, IXmlGPS>>(
                                       () => (x, y) => new XmlGPS(container.Resolve<IXPathMapBuilder>().DeserializeMap(x), y)));
            container.Register(Component.For<IFindBulkOperations>().ImplementedBy<FindBulkOperations>());
            container.Register(Component.For<IFindBulkOperationExceptions>().ImplementedBy<FindBulkOperationExceptions>());
            container.Register(Component.For<ISetBulkOperationStatus>().ImplementedBy<SetBulkOperationStatus>());
            container.Register(Component.For<IPersistBulkOperationExceptions>().ImplementedBy<PersistBulkOperationExceptions>());
            container.Register(Component.For<ISetUploadFileStatus>().ImplementedBy<SetUploadFileStatus>());
            container.Register(Component.For<ICreateBulkOperation>().ImplementedBy<CreateBulkOperation>());
            container.Register(Component.For<IDeleteUploadFileChunks>().ImplementedBy<DeleteUploadFileChunks>());

            RegisterUploadFileValidator(container);
        }

        public static void RegisterUploadFileValidator(IWindsorContainer container)
        {
            container.Register(Component.For<IValidator<UploadInfo>>()
                                        .ImplementedBy<UploadInfoValidator>());
        }
    }
}