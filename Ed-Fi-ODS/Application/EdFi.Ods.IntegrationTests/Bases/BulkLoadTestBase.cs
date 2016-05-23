using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.IO;
using EdFi.Ods.BulkLoad.Console;
using EdFi.Ods.BulkLoad.Core;
using EdFi.Ods.BulkLoad.Core.Controllers;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Models.Resources.Enums;
using EdFi.Ods.BulkLoad.Core.Data;
using NUnit.Framework;

namespace EdFi.Ods.IntegrationTests.Bases
{
    [TestFixture]
    public class BulkLoadTestBase
    {
        private string _directory;
        internal Guid _operationId;
        internal IEnumerable<KeyValuePair<string, Stream>> _sourceStreams;
        protected IWindsorContainer Container;

        [TestFixtureSetUp]
        public void Setup()
        {
            EstablishContext();
            if(!_sourceStreams.Any()) return;
            _operationId = Guid.NewGuid();
            _directory = Path.Combine(Path.GetTempPath(), _operationId.ToString());  //TODO: set the bulk operation working directory setting and use it here . . . 
            if (!Directory.Exists(_directory)) Directory.CreateDirectory(_directory);
            //We have to setup the files we'll be parsing for each TestFixture - do this only once per fixture
            var files = new List<KeyValuePair<string, string>>();
            var filenum = 0;
            var baseFileName = "TestXml" + DateTime.UtcNow.Ticks; //UploadFilenames must be unique

            //We'll write each stream to a file and save that file name in the manifest
            foreach (var typeStreamPair in _sourceStreams)
            {
                filenum++;
                var file = baseFileName + filenum + ".xml";
                files.Add(new KeyValuePair<string, string>(file, typeStreamPair.Key));
                var filepath = _directory + @"\" + file;
                using (var filestream = File.Create(filepath))
                {
                    typeStreamPair.Value.CopyTo(filestream);
                }
            }

            var access = new ConfigurationAccess();
            var connectionStringBuilder = new SqlConnectionStringBuilder(access.BaseOdsConnectionString);
            connectionStringBuilder.InitialCatalog = "EdFi_Ods_Sandbox_XmlTest_Minimal_Key"; //TODO: Really ought to be in a configuration setting . . .
            var config = new BulkLoaderConfiguration {OdsConnectionString = connectionStringBuilder.ToString()};
            Container = Bootstrapper.Bootstrap(config);

            var localBulkOperation = new LocalBulkOperationInitializer(
                new InterchangeFileTypeManifestTranslator(config.Manifest),
                Container.Resolve<ICreateBulkOperation>(),
                config,
                Container.Resolve<IFileSystem>())
                .CreateOperationAndGetLocalFiles();

            // TODO: GKM - This seems like an unusual use of IoC.  Why does this work here use the container as the mechanism for passing data around?
            Container.Register(Component
                .For<IEnumerable<LocalUploadFile>>()
                .Instance(localBulkOperation.LocalUploadFiles));

            //finally - we can create the operation
            var operation = new BulkOperation
            {
                DatabaseName = connectionStringBuilder.InitialCatalog,
                Id = _operationId.ToString(),
                Status = BulkOperationStatus.Ready,
                UploadFiles =
                    files.Select(
                        keyValuePair =>
                            new UploadFile
                            {
                                Id = keyValuePair.Key,
                                InterchangeType = keyValuePair.Value,
                                Status = UploadFileStatus.Ready,
                            }).ToList(),
            };
            var executor = Container.Resolve<IDbExecutor<IBulkOperationDbContext>>();
            executor.ApplyChanges(c => c.BulkOperations.Add(operation));
        }

        internal virtual void EstablishContext()
        {
            _sourceStreams = new List<KeyValuePair<string, Stream>>();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            if (!string.IsNullOrWhiteSpace(_directory) && Directory.Exists(_directory))
            {
                Directory.Delete(_directory, true);
            }

            if (_operationId == null || _operationId == Guid.Empty) return;

            var executor = Container.Resolve<IDbExecutor<IBulkOperationDbContext>>();
            executor.ApplyChanges(c =>
            {
                var operation = c.BulkOperations.Find(_operationId.ToString());
                if(operation == null) return;
                var uploadFiles = operation.UploadFiles.ToArray();
                for(int i=0;i<uploadFiles.Length;i++)
                {
                    c.UploadFiles.Remove(uploadFiles[i]);
                }
                c.BulkOperations.Remove(operation);
            });

            //Tear down IoC??
        }

        [Test]
        public void DummyTest()
        {
            Assert.Pass();//To make this class green instead of inconclusive in the unit tests window.
        }
        internal IControlBulkLoading GetBulkOperationCmdHandler()
        {
            return Container.Resolve<IControlBulkLoading>();
        }

        internal IEnumerable<BulkOperationException> GetExceptions()
        {
            var executor = Container.Resolve<IDbExecutor<IBulkOperationDbContext>>();
            IEnumerable<string> uploadFileIds = executor.Get(c =>
            {
                var op = c.BulkOperations.Find(_operationId.ToString());
                return op == null ? new List<string>() : op.UploadFiles.Select(u => u.Id).ToList();
            });
            return executor.Get(c =>
            {
                var exceptions = new List<BulkOperationException>();
                foreach (var id in uploadFileIds)
                {
                    exceptions.AddRange(c.BulkOperationExceptions.Where(e => e.ParentUploadFileId == id));
                }
                return exceptions;
            });
        }
    }
}