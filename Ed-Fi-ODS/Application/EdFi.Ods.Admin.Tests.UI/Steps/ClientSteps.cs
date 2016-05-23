namespace EdFi.Ods.Admin.UITests.Steps
{
    using System;

    using EdFi.Ods.Admin.UITests.Pages;
    using EdFi.Ods.Admin.UITests.Support;

    using NUnit.Framework;

    using TechTalk.SpecFlow;

    [Binding]
    public class ClientSteps
    {
        private readonly ClientPage _clientPage;

        public ClientSteps(ClientPage clientpage)
        {
            this._clientPage = clientpage;
        }

        [Given(@"I go to the Client Applications page")]
        public void GivenIGoToTheClientApplicationsPage()
        {
            this._clientPage.Visit();
        }

        [When(@"I add an Application with ""(sample|minimal)"" data")]
        public void WhenIAddAApplicationWithTheNameAndType(string sandboxType)
        {
            bool useSampleData = sandboxType == "sample";
            string name = "Application" + DateTime.Now.Ticks;
            this._clientPage.CreateNewApplication(name, useSampleData);
            ScenarioContext.Current.Add("applicationName", name);
            ScenarioContext.Current.Add("type", useSampleData);
        }

        [Then(@"The application should appear on the Existing Applications Table")]
        public void ThenTheApplicationShouldAppearOnTheExistingApplicationsTable()
        {
            bool sampledata = ScenarioContext.Current.Get<bool>("type");
            string name = ScenarioContext.Current.Get<string>("applicationName");

            Try.WaitingForThis(
                () => this._clientPage.IsApplicationInTable(name), 
                TimeSpan.FromMinutes(1));

            Assert.That(this._clientPage.IsApplicationInTable(name));

            Try.WaitingForThis(
                () => this._clientPage.SampleDataIsInUse(name) == sampledata,
                TimeSpan.FromSeconds(5));

            Assert.That(this._clientPage.SampleDataIsInUse(name) == sampledata);
        }

        [Then(@"The application should not appear on the Existing Applications Table")]
        public void ThenTheApplicationShouldNotAppearOnTheExistingApplicationsTable()
        {
            string name = ScenarioContext.Current.Get<string>("applicationName");

            Try.WaitingForThis(
                () => !this._clientPage.IsApplicationInTable(name),
                TimeSpan.FromSeconds(5));

            Assert.That(!this._clientPage.IsApplicationInTable(name));
        }


        [When(@"I delete the application")]
        public void WhenIDeleteTheApplication()
        {
            this._clientPage.DeleteSandbox(ScenarioContext.Current.Get<string>("applicationName"));
        }

        [When(@"I update the application")]
        public void WhenIUpdateTheApplication()
        {
            string name = ScenarioContext.Current.Get<string>("applicationName");
            KeyPair keys = this._clientPage.getKeyandSecret(name);
            ScenarioContext.Current.Add("OldKeys", keys);
            this._clientPage.UpdateKeys(name);
        }

        [Then(@"The application's keys shouldn't be the same")]
        public void ThenTheApplicationTBeTheSame()
        {
            string name = ScenarioContext.Current.Get<string>("applicationName");
            KeyPair newKeys = this._clientPage.getKeyandSecret(name);
            Assert.That(newKeys.Secret != ScenarioContext.Current.Get<KeyPair>("OldKeys").Secret);
        }
    }
}