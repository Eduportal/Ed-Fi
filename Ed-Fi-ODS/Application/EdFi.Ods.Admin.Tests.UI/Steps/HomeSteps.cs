namespace EdFi.Ods.Admin.UITests.Steps
{
    using EdFi.Ods.Admin.UITests.Pages;

    using TechTalk.SpecFlow;

    [Binding]
    public class HomeSteps
    {
        private readonly HomePage homePage;

        public HomeSteps(HomePage homePage)
        {
            this.homePage = homePage;
        }

        [Given(@"I am on the Admin console home page")]
        public void GivenIAmOnTheAdminConsoleHomePage()
        {
            this.homePage.Visit();
        }
    }
}