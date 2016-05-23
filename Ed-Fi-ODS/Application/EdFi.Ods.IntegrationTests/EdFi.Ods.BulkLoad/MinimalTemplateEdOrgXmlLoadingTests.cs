using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.IntegrationTests.Bases;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Messaging.BulkLoadCommands;
using NUnit.Framework;
using Should;
using EdFi.Ods.Tests.TestObjects.TestXml;

namespace EdFi.Ods.IntegrationTests.EdFi.Ods.BulkLoad
{
    [TestFixture]
    [Ignore("This Test serves only as a utility to call or debug Bulkload functionality, which otherwise needs to be invoked through the Wep API or Console App.")]
    public class MinimalTemplateEdOrgXmlLoadingTests : BulkLoadTestBase
    {
        internal override void EstablishContext()
        {
            _sourceStreams = new[]
            {
                MinimalTemplateEdOrgXml.EdOrgXmlTypePair
            };
        }

        [Test]
        public void Given_EdOrgInterchangeXml_Should_Load_It()
        {
            var cmd = new StartOperationCommand { OperationId = _operationId };
            var sut = GetBulkOperationCmdHandler();
            sut.Handle(cmd);
            var exceptions = GetExceptions() as List<BulkOperationException>;
            Console.WriteLine(@"{0} exceptions occurred while loading the supplied Xml.", exceptions.Count);
            exceptions.ForEach(e => Console.WriteLine(@"Element: {0} was not loaded due to: {1}", e.Element, e.Message));
            exceptions.Any().ShouldBeFalse();
        }
    }
}
