namespace EdFi.Ods.Admin.UITests.Pages.GlobalPages
{
    using Coypu;

    public abstract class PartialPageBase
    {
        protected PartialPageBase(BrowserSession browser)
        {
            this.Browser = browser;
        }

        public BrowserSession Browser { get; private set; }
    }
}