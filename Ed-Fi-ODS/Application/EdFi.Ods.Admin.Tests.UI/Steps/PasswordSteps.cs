namespace EdFi.Ods.Admin.UITests.Steps
{
    using System;

    using EdFi.Ods.Admin.UITests.Pages;
    using EdFi.Ods.Admin.UITests.Pages.GlobalPages;

    using NUnit.Framework;

    using TechTalk.SpecFlow;

    [Binding]
    public class PasswordSteps
    {
        private readonly PasswordChangePage passwordChangePage;
        private readonly MastheadPage _mastheadPage;
        private readonly ForgotPasswordPage _forgotPasswordPage;

        public PasswordSteps(PasswordChangePage passwordChangePage, MastheadPage mastheadPage, ForgotPasswordPage forgotPasswordPage)
        {
            this.passwordChangePage = passwordChangePage;
            this._mastheadPage = mastheadPage;
            this._forgotPasswordPage = forgotPasswordPage;
        }

        [Given(@"I am on the password change page")]
        public void GivenIAmOnThePasswordChangePage()
        {
            this.passwordChangePage.Visit();
        }
        
        [Given(@"I am on the forgot password page")]
        public void GivenIAmOnTheForgotPasswordPage()
        {
            this._forgotPasswordPage.Visit();
        }

        [When(@"I send the reset email to ""(.*)""")]
        public void WhenISendTheResetEmailTo(string email)
        {
            this._forgotPasswordPage.SendEmail(email);
        }
        
        [When(@"I change my password to ""(.*)""")]
        public void WhenIChangeMyPasswordFromTo(string newPassword)
        {
            var oldPassword = FeatureContext.Current.Get<string>("password");
            this.passwordChangePage.ChangePassword(oldPassword, newPassword);
            FeatureContext.Current.Remove("password");
        }

        [Then(@"""(.*)"" should be logged in")]
        public void ThenShouldBeLoggedIn(string name)
        {
            Assert.That(name == this._mastheadPage.GetLoggedInUser());
        }

        [Given(@"No users are logged in")]
        public void NoUsersAreLoggedIn()
        {
            // Only log out if necessary
            if (this._mastheadPage.IsLoggedIn())
            {
                try { this._mastheadPage.LogOut(); }
                catch (Exception ex) { Console.WriteLine("Eaten Exception: {0}", ex.ToString()); }
            }
        }

        [When(@"I log out")]
        public void WhenILogOut()
        {
            this._mastheadPage.LogOut();
        }

        [When(@"I click OK")]
        public void WhenIClickOK()
        {
            this.passwordChangePage.ClickOk();
        }

        [When(@"I type in the invalid old password")]
        public void WhenITypeInTheInvalidOldPassword()
        {
            this.passwordChangePage.EnterCurrentPassword("a");
        }

        [When(@"I type in the (Invalid|Correct) new password")]
        public void WhenITypeInTheInvalidNewPassword(string invalidOrCorrect)
        {
            this.passwordChangePage.EnterNewPassword(invalidOrCorrect == "Invalid" ? "a" : "1234567Eight!");
        }

        [When(@"I type in the (Invalid|Correct) confirm password")]
        public void WhenITypeInTheInvalidConfirmPassword(string invalidOrCorrect)
        {
            this.passwordChangePage.EnterConfirmPassword(invalidOrCorrect == "Invalid" ? "a" : "1234567Eight!");
        }
    }
}