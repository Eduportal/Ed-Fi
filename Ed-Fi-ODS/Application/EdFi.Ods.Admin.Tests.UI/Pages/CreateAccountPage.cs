namespace EdFi.Ods.Admin.UITests.Pages
{
    using Coypu;

    using EdFi.Ods.Admin.UITests.Attributes;
    using EdFi.Ods.Admin.UITests.Support.Coypu;
    using EdFi.Ods.Common.Utils.Extensions;

    [AssociatedUrl("^/Account/Create.*$")]
    public class CreateAccountPage : PageBase
    {
        public CreateAccountPage(BrowserSession browser) : base(browser)
        {
        }

        public override void Visit()
        {
            if (!(this.IsCurrent()))
                this.Visit("~/Account/Create");
        }

        public void CreateAccount(string name, string email)
        {
            this.Browser.FillIn("name").With(name);
            this.Browser.FillIn("email").With(email);
            this.Browser.ClickButton("Create Account");

            // Wait for the button to disappear before continuing
            this.Browser.RetryUntilTimeout(
                () => this.Browser.HasNoContent("Create Account"), 
                Make.Options.Wait(10.Seconds()));
        }
    }
}