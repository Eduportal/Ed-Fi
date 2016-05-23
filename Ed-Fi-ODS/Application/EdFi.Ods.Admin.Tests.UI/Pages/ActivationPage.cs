namespace EdFi.Ods.Admin.UITests.Pages
{
    using Coypu;

    using EdFi.Ods.Admin.UITests.Attributes;

    [AssociatedUrl("^/Account/ActivateAccount.*$")]
    public class ActivationPage : PageBase
    {
        public ActivationPage(BrowserSession browser) : base(browser)
        {
        }

        public override void Visit()
        {
            if (!(this.IsCurrent()))
                this.Visit("~/Account/ActivateAccount");
        }

        protected override void Visit(string activationMarker)
        {
            if (!(this.IsCurrent()))
                this.Visit("~/Account/ActivateAccount");
        }

        public void FillInPasswordAndConfirm(string password)
        {
            this.Browser.FillIn("password").With(password);
            this.Browser.FillIn("confirmpassword").With(password);
        }

        public void ClickActivateButton()
        {
            this.Browser.ClickButton("btn-activate-account");
        }

        public void ClickResetButton()
        {
            this.Browser.ClickButton("btn-reset-password");
        }
    }
}