namespace EdFi.Ods.Admin.UITests.Pages
{
    using Coypu;

    using EdFi.Ods.Admin.UITests.Attributes;
    using EdFi.Ods.Admin.UITests.Support.Coypu;
    using EdFi.Ods.Common.Utils.Extensions;

    [AssociatedUrl("^/Account/ChangePassword.*$")]
    public class PasswordChangePage : PageBase
    {
        public PasswordChangePage(BrowserSession browser) : base(browser)
        {
        }

        public virtual string CurrentPasswordCss
        {
            get { return "#currentpassword"; }
        }

        public virtual string NewPasswordCss
        {
            get { return "#newpassword"; }
        }

        public virtual string SecondNewPasswordCss
        {
            get { return "#confirmpassword"; }
        }

        public override void Visit()
        {
            if (!this.IsCurrent())
                this.Visit("~/Account/ChangePassword");
        }

        public void ChangePassword(string oldPassword, string newPassword)
        {
            // Wait for browser to catch up
            this.Browser.RetryUntilTimeout(
                () => this.Browser.FindCss(this.CurrentPasswordCss).Exists(),
                Make.Options.Wait(5.Seconds()));

            this.EnterCurrentPassword(oldPassword);
            this.EnterNewPassword(newPassword);
            this.EnterConfirmPassword(newPassword);
            this.Browser.ClickButton("Reset Password");
        }

        public void ClickOk()
        {
            this.Browser.ClickButton("Reset Password");
        }

        public void EnterCurrentPassword(string currentPassword)
        {
            this.Browser.FindCss(this.CurrentPasswordCss).FillInWith(currentPassword);
        }

        public void EnterNewPassword(string newPassword)
        {
            this.Browser.FindCss(this.NewPasswordCss).FillInWith(newPassword);
        }

        public void EnterConfirmPassword(string newPassword)
        {
            this.Browser.FindCss(this.SecondNewPasswordCss).FillInWith(newPassword);
        }
    }
}