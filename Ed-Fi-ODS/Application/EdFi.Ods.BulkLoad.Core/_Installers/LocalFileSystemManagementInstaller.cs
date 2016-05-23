using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EdFi.Ods.Api.Data.Repositories.BulkOperations;
using EdFi.Ods.Common;

namespace EdFi.Ods.BulkLoad.Core._Installers
{
    public class LocalFileSystemManagementInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IPersistUploadFiles>().ImplementedBy<PersistUploadFilesLocally>());
            container.Register(Component.For<ICreateFilePathForUploadFile>().ImplementedBy<CreateFilePathForUploadFileLocally>());
            container.Register(Component.For<IStreamFileChunksToWriter>().ImplementedBy<SqlStreamFileChunksToWriter>());
            container.Register(Component.For<IValidateAndSourceFiles>().ImplementedBy<ValidateAndSourceFiles>());
        }
    }
}