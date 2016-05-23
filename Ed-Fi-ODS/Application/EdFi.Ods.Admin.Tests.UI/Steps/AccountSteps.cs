namespace EdFi.Ods.Admin.UITests.Steps

{
    using System;
    using System.Linq;

    using EdFi.Ods.Admin.UITests.Pages;
    using EdFi.Ods.Admin.UITests.Pages.GlobalPages;
    using EdFi.Ods.Admin.UITests.Support.Account;
    using EdFi.Ods.Admin.UITests.Support.Extensions;

    using Should;

    using TechTalk.SpecFlow;

    [Binding]
    public class AccountSteps
    {
        private readonly AccountManager _accountManager;
        private readonly LoginPage _loginPage;
        private readonly CreateAccountPage _createAccountPage;
        private readonly MastheadPage _mastheadPage;
        private readonly IEmailReader _emailReader;
        private readonly HomePage _homePage;
        private readonly ResendActivationPage _resendActivationPage;
        private readonly ActivationPage _activatePage;

        public AccountSteps(AccountManager accountManager, LoginPage loginPage, CreateAccountPage createAccountPage,
                            MastheadPage mastheadPage, IEmailReader emailReader, HomePage homePage, ResendActivationPage resendActivationPage,
                            ActivationPage activatePage)
        {
            this._accountManager = accountManager;
            this._loginPage = loginPage;
            this._createAccountPage = createAccountPage;
            this._mastheadPage = mastheadPage;
            this._emailReader = emailReader;
            this._homePage = homePage;
            this._resendActivationPage = resendActivationPage;
            this._activatePage = activatePage;
        }

        [Given(@"The account ""(.*)"" does not exist")]
        public void GivenTheAccountDoesNotExist(string email)
        {
            this._accountManager.RemoveAccount(email);
        }

        [Given(@"There exists a regular user with name ""(.*)"" password ""(.*)"" email ""(.*)""")]
        public void GivenThereExistsAUserWithNamePasswordEmail(string name, string password, string email)
        {
            this._accountManager.CreateAccount(new Account {Email = email, Name = name, Password = password});
        }

        [Given(@"There exists a user with name ""(.*)"" password ""(.*)"" email ""(.*)"" and roles{0,1} ""(.*)""")]
        public void GivenThereExistsAUserWithNameAndEmailAndRole(string name, string password, string email, string roles)
        {
            var rolesArray = roles.Split(',').Select(x => x.Trim()).ToArray();
            this._accountManager.CreateAccount(new Account {Email = email, Name = name, Roles = rolesArray, Password = password});
        }

        [Given(@"I am logged in successfully with email ""(.*)"" and password ""(.*)""")]
        [When(@"I log in successfully with email ""(.*)"" and password ""(.*)""")]
        public void WhenILogInSuccessfullyWithEmailAndPassword(string email, string password)
        {
            this.WhenILogInWithEmailAndPassword(email, password);

            // Make sure login was successful, and if not, snap a screenshot of the problem
            if (string.IsNullOrEmpty(this._mastheadPage.GetLoggedInUser()))
                throw new Exception("Login failed: " + this._loginPage.Message);
        }

        [Given(@"I am logged in with email ""(.*)"" and password ""(.*)""")]
        [When(@"I log in with email ""(.*)"" and password ""(.*)""")]
        public void WhenILogInWithEmailAndPassword(string email, string password)
        {
            if (this._homePage.Browser.HasNoAddress())
                this._homePage.Visit();

            this._loginPage.Login(email, password);

            FeatureContext.Current.Set(password, "password");
        }

        [Then(@"I should see the text ""(.*)"" in the masthead")]
        public void ThenIShouldSeeTheTextInTheMasthead(string expectedText)
        {
            this._mastheadPage.HasContent(expectedText).ShouldBeTrue();
        }

        [Then(@"I should not see the text ""(.*)"" in the masthead")]
        public void ThenIShouldNotSeeTheTextInTheMasthead(string text)
        {
            this._mastheadPage.DoesNotHaveMastheadContent(text).ShouldBeTrue();
        }

        [When(@"I create an account with name ""(.*)"" and email ""(.*)""")]
        public void WhenICreateAnAccountWithNameAndEmail(string name, string emailAddress)
        {
            this._createAccountPage.Visit();
            this._createAccountPage.CreateAccount(name, emailAddress);
        }

        [When(@"I follow the activation link sent to ""(.*)""")]
        public void WhenIFollowTheActivationLinkSentTo(string emailAddress)
        {
            var activationLink = this._emailReader.FindLink(emailAddress);
            ScenarioContext.Current.Set(activationLink, "ActivationLink_" + emailAddress);
            Console.WriteLine("Activation Link: [{0}]", activationLink);

            this._createAccountPage.Browser.Visit(activationLink);
        }

        [When(@"I reuse the activation link sent to ""(.*)""")]
        public void WhenIReuseTheActivationLinkSentTo(string emailAddress)
        {
            string activationLink = ScenarioContext.Current.Get<string>("ActivationLink_" + emailAddress);
            Console.WriteLine("Reused Activation Link: [{0}]", activationLink);

            this._createAccountPage.Browser.Visit(activationLink);
        }

        [When(@"I follow the forgot password link sent to ""(.*)""")]
        public void WhenIFollowTheForgotPasswordLinkSentTo(string emailAddress)
        {
            var activationLink = this._emailReader.FindLink(emailAddress);
            Console.WriteLine("Activation Link: [{0}]", activationLink);
            this._createAccountPage.Browser.Visit(activationLink);
        }

        [When(@"I resend account activation to ""(.*)""")]
        public void WhenIResendAccountActivationTo(string emailAddress)
        {
            this._resendActivationPage.Visit();
            this._resendActivationPage.ResendActivation(emailAddress);
        }

        [When(@"I activate the account with password ""(.*)""")]
        public void WhenIActivateTheAccountWithPassword(string password)
        {
            this._activatePage.FillInPasswordAndConfirm(password);
            this._activatePage.ClickActivateButton();
        }

        [When(@"I reset the password to ""(.*)""")]
        public void WhenIResetThePasswordTo(string password)
        {
            this._activatePage.FillInPasswordAndConfirm(password);
            this._activatePage.ClickResetButton();
        }
    }
}