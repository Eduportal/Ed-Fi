namespace EdFi.Ods.Admin.UITests.Pages.GlobalPages
{
    using Coypu;

    using EdFi.Ods.Admin.UITests.Support.Coypu;
    using EdFi.Ods.Common.Utils.Extensions;

    using Should;

    public class LoginPage : PartialPageBase
    {
        private readonly MastheadPage _mastheadPage;
        private const string HtmlId = "#loginModal";

        public LoginPage(MastheadPage mastheadPage, BrowserSession browser)
            : base(browser)
        {
            this._mastheadPage = mastheadPage;
        }

        private ElementScope GetModal()
        {
            return this.Browser.FindCss(HtmlId);
        }

        public void Login(string email, string password)
        {
            this._mastheadPage.ShowLogin();
            this.GetModal().FillIn("emailaddress").With(email);
            this.GetModal().FillIn("password").With(password);
            this.GetModal().ClickButton("Login");
            this.GetModal().Missing().ShouldBeTrue("Login modal should have closed.  Perhaps the login failed.");
        }

        public string Message
        {
            get
            {
                try
                {
                    var messageLabel = this.Browser.FindId("message-login", Make.Options.Wait(2.Seconds()));

                    if (messageLabel != null)
                        return messageLabel.Text;
                }
                catch { }

                return string.Empty;
            }
        }

        public bool IsVisible
        {
            get 
            {
                try
                {
                    var btn = this.Browser.FindId("btn-login", Make.Options.Do_It_Now);

                    return (btn != null);
                }
                catch { return false; }
            }
        }
    }
}