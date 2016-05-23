namespace EdFi.Ods.Admin.UITests.Steps
{
    using Coypu;

    using EdFi.Ods.Admin.UITests.Pages;
    using EdFi.Ods.Admin.UITests.Pages.GlobalPages;
    using EdFi.Ods.Admin.UITests.Support.Navigation;

    using Should;

    using TechTalk.SpecFlow;

    [Binding]
    public class GeneralSteps
    {
        private readonly ActivationPage _ActivationPage;
        private readonly MainContentPage _mainContentPage;
        private readonly BrowserSession _browser;

        public GeneralSteps(ActivationPage ActivationPage, MainContentPage mainContentPage, BrowserSession browser)
        {
            this._ActivationPage = ActivationPage;
            this._mainContentPage = mainContentPage;
            this._browser = browser;
        }

        [Then(@"I should see an error that says ""(.*)""")]
        public void ThenIShouldSeeAnErrorThatSays(string errorMessage)
        {
            this._browser.FindCss("div.alert-danger").HasContent(errorMessage).ShouldBeTrue();
        }

        [Then(@"I should see a warning that says ""(.*)""")]
        public void ThenIShouldSeeAWarningThatSays(string warningMessage)
        {
            this._browser.FindCss("div.alert").HasContent(warningMessage).ShouldBeTrue();
        }

        [When(@"I visit the ""(.*)"" page")]
        public void WhenIVisitThePage(string pageName)
        {
            var page = PageRegistry.GetPage(pageName);
            page.Visit();
        }

        [Then(@"I should see text that says ""(.*)""")]
        public void ThenIShouldSeeAMessageThatSays(string messageText)
        {
            this._mainContentPage.HasContent(messageText).ShouldBeTrue();
        }
    }
}