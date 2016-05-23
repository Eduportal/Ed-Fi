using Castle.Windsor;

namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Castle.MicroKernel.Registration;

    using global::EdFi.Common.InversionOfControl;
    using global::EdFi.Ods.BulkLoad.Common;
    using global::EdFi.Ods.BulkLoad.Common.OrderProviders;
    using global::EdFi.Ods.BulkLoad.Core;
    using global::EdFi.Ods.CodeGen.XmlShredding;
    using global::EdFi.Ods.CodeGen.XsdToWebApi;
    using global::EdFi.Ods.Common;
    using global::EdFi.Ods.Pipelines;
    using global::EdFi.Ods.Pipelines.Factories;
    using global::EdFi.Ods.Tests._Bases;
    using global::EdFi.Ods.XmlShredding;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    /// <summary>
    /// This is a fragile test to cover the behavior of a fragile class.  The test's
    /// intent is to protect the Bulk Processing from being inadvertently broken.
    /// The goal was to tie which loaders to use in which Interchange to the Xsd for 
    /// that class.  The resource factories and aggregate loader classes that use them
    /// are generated using this same method so it was felt this was keep them aligned.
    /// 
    /// If you have an idea that will support the original goal and be more modular/testable
    /// please do so . . 
    /// </summary>
    [TestFixture]
    public class HaveInterchangeSpecificLoadersTests : TestBase
    {
        [Test]
        public void Covering_Test_to_Ensure_All_Found_Xsds_Are_Present_in_Collections_Model()
        {
            var files = Directory.GetFiles(@"..\..\..\schema.codegen");
            if (!files.Any()) Assert.Fail("No XSD files where found");
            var interchangeObjects = (from xsdPath in files select InterchangeLoader.Load(xsdPath) into results where results.Any() select results.Single()).ToList();
            var xsdFileProvider = this.Stub<IInterchangeXsdFileProvider>();
            xsdFileProvider.Expect(f => f.GetInterchangeFilePaths()).IgnoreArguments().Return(files);
            var dictionary = new Dictionary<string, IOrderedCollectionofAggregateSetsBuilder>(AggregateLoadOrderProviders.Dictionary);
            var sut = new InterchangeSpecificLoaderCollectionTemplateModel(files, dictionary);
            foreach (var schemaObject in interchangeObjects)
            {
                var interchangeName = schemaObject.XmlSchemaObjectName.Substring(11).ToLowerInvariant();
                InterchangeType.GetByName(interchangeName)
                    .ShouldNotBeNull(string.Format("{0} did not match a known Interchange Type Name.", interchangeName));

                CollectionAssert.IsNotEmpty(sut.CollectionFor(interchangeName),
                    "{0} did not generate any aggregate loaders.", schemaObject.XmlSchemaObjectName);
            }
        }
    }
}