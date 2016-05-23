namespace EdFi.Ods.Admin.UITests.Pages
{
    using Coypu;

    using EdFi.Ods.Admin.UITests.Attributes;

    [AssociatedUrl("/Account/ForgotPassword")]
    public class ForgotPasswordPage : PageBase
    {
        public ForgotPasswordPage(BrowserSession browser) : base(browser)
        {
        }

        public override void Visit()
        {
            if (!(this.IsCurrent()))
                this.Visit("~//Account/ForgotPassword");
        }

        public void SendEmail(string email)
        {
            this.Browser.FindCss("#input-forgot-password-email").FillInWith(email);
            this.Browser.FindCss("#btn-forgot-password-reset").Click();
        }
    }
}