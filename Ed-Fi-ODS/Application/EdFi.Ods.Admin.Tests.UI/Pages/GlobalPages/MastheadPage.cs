namespace EdFi.Ods.Admin.UITests.Pages.GlobalPages
{
    using System;

    using Coypu;

    using EdFi.Ods.Admin.UITests.Support.Coypu;
    using EdFi.Ods.Common.Utils.Extensions;

    using NUnit.Framework;

    public class MastheadPage : PartialPageBase
    {
        private const string HtmlId = "#masthead";

        public MastheadPage(BrowserSession browser) : base(browser)
        {
        }

        public bool HasContent(string content)
        {
            return this.Browser.FindCss(HtmlId).HasContent(content);
        }

        public bool DoesNotHaveMastheadContent(string content)
        {
            return this.GetMasthead().HasNoContent(content);
        }

        private ElementScope GetMasthead()
        {
            return this.Browser.FindCss(HtmlId);
        }

        public void LogOut()
        {
            if (this.GetMasthead().HasNoContent("Login"))
            {
                this.GetMasthead().FindCss("#account-menu").Click();
                this.GetMasthead()
                    .FindCss("#account-menu-body")
                    .WaitForAnimation()
                    .FindCss("#btn-logout")
                    .Click();
//                Thread.Sleep(250);
//                GetMasthead().FindCss("#btn-logout", Make_It.Wait_2_Seconds).Click();
            }
            else
            {
                Assert.Fail("Attempting to logout when no user is authenticated.");
            }
        }

        public string GetLoggedInUser()
        {
            string loggedinuser = string.Empty;
            try
            {
                loggedinuser = this.Browser.FindCss("span.authUserName", Make.Options.Wait(2.Seconds())).Text;
            }
            catch (Exception)
            {
            }

            return loggedinuser;
        }

        public virtual bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(this.GetLoggedInUser());
        }

        public virtual bool IsLoggedInAs(string username)
        {
            string currentUsername = this.GetLoggedInUser();
            return currentUsername == username;
        }

        public void ShowLogin()
        {
            //GetMasthead().ClickLink("Login"); //It seems this doesn't always work for some reason. Huh.
            this.Browser.FindCss("#btn-showLogin").Click();
        }
    }
}