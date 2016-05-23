namespace EdFi.Ods.Admin.UITests.Steps
{
    using Coypu;

    using EdFi.Ods.Admin.UITests.Pages;

    using Should;

    using TechTalk.SpecFlow;

    [Binding]
    public class OrphanSteps
    {
        private readonly BrowserSession _browser;
        private readonly OrphanReportPage _orphanPage;

        public OrphanSteps(BrowserSession browser, OrphanReportPage orphanPage)
        {
            this._browser = browser;
            this._orphanPage = orphanPage;
        }

        [When(@"I clear all orphan databases")]
        public void WhenIClearAllOrphanDatabases()
        {
            this._orphanPage.ClearOrphans();
        }

        [Then(@"I should have ""(.*)"" orphan databases")]
        public void ThenIShouldHaveOrphanDatabases(int expectedCount)
        {
            this._orphanPage.GetOrphanCount().ShouldEqual(expectedCount);
        }
    }
}