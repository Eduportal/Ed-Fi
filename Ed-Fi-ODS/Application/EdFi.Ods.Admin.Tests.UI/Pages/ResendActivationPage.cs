namespace EdFi.Ods.Admin.UITests.Pages
{
    using Coypu;

    using EdFi.Ods.Admin.UITests.Attributes;

    [AssociatedUrl("^/Account/ResendAccountActivation.*$")]
    public class ResendActivationPage : PageBase
    {
        public ResendActivationPage(BrowserSession browser) : base(browser)
        {
        }

        public override void Visit()
        {
            if (!(this.IsCurrent()))
                this.Visit("~/Account/ResendAccountActivation");
        }

        public void ResendActivation(string emailAddress)
        {
            this.Browser.FillIn("email").With(emailAddress);
            this.Browser.ClickButton("Send Activation");
        }
    }
}