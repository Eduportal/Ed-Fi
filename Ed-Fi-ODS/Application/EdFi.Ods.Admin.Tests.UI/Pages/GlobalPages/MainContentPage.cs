namespace EdFi.Ods.Admin.UITests.Pages.GlobalPages
{
    using Coypu;

    public class MainContentPage : PartialPageBase
    {
        private const string HtmlId = "#main-content";

        public MainContentPage(BrowserSession browser) : base(browser)
        {
        }

        public bool HasContent(string content)
        {
            return this.Browser.FindCss(HtmlId).HasContent(content);
        }
    }
}