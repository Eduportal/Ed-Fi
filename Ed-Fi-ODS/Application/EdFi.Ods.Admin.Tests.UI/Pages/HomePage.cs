namespace EdFi.Ods.Admin.UITests.Pages
{
    using Coypu;

    using EdFi.Ods.Admin.UITests.Attributes;

    [AssociatedUrl("^/$")]
    public class HomePage : PageBase
    {
        public HomePage(BrowserSession browser) : base(browser)
        {
        }

        public override void Visit()
        {
            if (!(this.IsCurrent()))
                this.Visit("~/");
        }
    }
}