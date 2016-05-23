using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using EdFi.Common.Messaging;
using EdFi.Common.Security.Claims;
using EdFi.Messaging.MSMQ;
using EdFi.Ods.CodeGen.XsdToWebApi.Process;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.EdFi.Messaging.MSMQ.MsmqMessageFactoryTests
{
    [TestFixture]
    public class When_Creating_A_Message
    {
        private MsmqMessageFactory sut;
        private Message result;
        private IEnvelope<TestMessage> givenCommand;
        private string expectedMessageProperty = "Expected";
                
        [TestFixtureSetUp]
        public void Arrange_And_Act()
        {
            sut = new MsmqMessageFactory();
            givenCommand = new Envelope<TestMessage>(new TestMessage(expectedMessageProperty));
            result = sut.Create(givenCommand);
        }

        [Test]
        public void Should_Put_Given_Command_Or_Event_Envelope_In_Message_Body()
        {
            result.Body.ShouldEqual(givenCommand);
        }

        [Test]
        public void Should_Set_Message_To_Be_Recoverable()
        {
            result.Recoverable.ShouldBeTrue();
        }

        [Test]
        public void Should_Set_Message_To_Use_Dead_Letter_Queue()
        {
            result.UseDeadLetterQueue.ShouldBeTrue();
        }
    }

    [TestFixture]
    public class When_Creating_An_Envelope_From_An_MSMQ_Message
    {
        private IEnvelope<TestMessage> given;
        private IEnvelope<TestMessage> result;

        [TestFixtureSetUp]
        public void ArrangeAndAct()
        {
            //Arrange
            given = new Envelope<TestMessage>(new TestMessage("Expected"))
            {
                EducationOrganizationIds = new[] {1, 2, 3},
                SchoolYear = 2014,
                ApiKey = "IAmKey",
                ClaimSetName = "SIS Vendor",
                NamespacePrefix = "http://www.TEST.org",
            };

            //Act
            var sut = new MsmqMessageFactory();
            var message = sut.Create(given);
            result = sut.GetEnvelope<TestMessage>(message);
        }
            
        [Test]
        public void Should_Contain_All_Supplied_LEA_Ids()
        {
            var matchedIds = given.EducationOrganizationIds.Count(educationOrganizationId => result.EducationOrganizationIds.Contains(educationOrganizationId));
            matchedIds.ShouldEqual(given.EducationOrganizationIds.Count());
        }

        [Test]
        public void Should_Contain_The_Given_SchoolYear()
        {
            result.SchoolYear.ShouldEqual(given.SchoolYear);
        }

        [Test]
        public void Should_Contain_The_Given_ApiKey()
        {
            result.ApiKey.ShouldEqual(given.ApiKey);
        }

        [Test]
        public void Should_Contain_The_Given_Message()
        {
            result.Message.GivenProperty.ShouldEqual(given.Message.GivenProperty);
        }

        [Test]
        public void Should_Contain_The_Given_ClaimSetName()
        {
            result.ClaimSetName.ShouldEqual(given.ClaimSetName);
        }

        [Test]
        public void Should_Contain_The_Given_NamespacePrefix()
        {
            result.NamespacePrefix.ShouldEqual(given.NamespacePrefix);
        }
    }

    public class TestMessage : ICommand
    {
        public string GivenProperty { get; set; }

        public TestMessage(string message)
        {
            GivenProperty = message;
        }
    }
}