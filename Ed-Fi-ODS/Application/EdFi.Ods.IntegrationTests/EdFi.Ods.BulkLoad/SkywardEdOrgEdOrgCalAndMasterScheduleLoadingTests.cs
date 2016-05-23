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
    [Ignore("Xml is invalid - fix and re-run")]
    public class SkywardEdOrgEdOrgCalAndMasterScheduleLoadingTests : BulkLoadTestBase
    {
        internal override void EstablishContext()
        {
            _sourceStreams = new[]
            {
                SkywardTestXml.EdOrgXmlTypePair, SkywardTestXml.EdOrgCalendarTypePair,
                SkywardTestXml.MasterScheduleTypePair
            };
        }

        [Test]
        public void Given_Skyward_Xml_For_EdOrg_EdOrgCal_And_MasterSchedule_Should_Load_Them_All()
        {
            var cmd = new StartOperationCommand {OperationId = _operationId};
            var sut = GetBulkOperationCmdHandler();
            sut.Handle(cmd);
            var exceptions = GetExceptions() as List<BulkOperationException>;
            Console.WriteLine(@"{0} exceptions occurred while loading the supplied Xml.", exceptions.Count);
            exceptions.ForEach(e => Console.WriteLine(@"Element: {0} was not loaded due to: {1}", e.Element, e.Message));
            exceptions.Any().ShouldBeFalse();
        }
    }
}