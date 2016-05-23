namespace EdFi.Ods.Admin.UITests.Steps
{
    using EdFi.Ods.Admin.UITests.Support.Account;

    using TechTalk.SpecFlow;

    [Binding]
    public class EmailSteps
    {
        private readonly IEmailReader _reader;

        public EmailSteps(IEmailReader reader)
        {
            this._reader = reader;
        }

        [Given(@"Test User Bill has no email in ""(.*)""")]
        public void GivenTestUserBillHasNoEmailIn(string email)
        {
            this._reader.ClearMailbox(email);
        }

        [Given(@"Test User Joe has no email in ""(.*)""")]
        public void GivenTestUserJoeHasNoEmailIn(string email)
        {
            this._reader.ClearMailbox(email);
        }

        [Given(@"Test User EdFi has no email in ""(.*)""")]
        public void GivenTestUserEdFiHasNoEmailIn(string email)
        {
            this._reader.ClearMailbox(email);
        }

        [Given(@"Test User Sara has no email in ""(.*)""")]
        public void GivenTestUserSaraHasNoEmailIn(string email)
        {
            this._reader.ClearMailbox(email);
        }

        [When(@"I begin testing")]
        public void WhenIBeginTesting()
        {
        }

        [Then(@"They should not have any emails from DLP")]
        public void ThenTheyShouldNotHaveAnyEmailsFromDLP()
        {
        }

    }
}