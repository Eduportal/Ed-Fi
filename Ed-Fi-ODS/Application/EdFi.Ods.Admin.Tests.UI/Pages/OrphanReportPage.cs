namespace EdFi.Ods.Admin.UITests.Pages
{
    using System;

    using Coypu;
    using Coypu.NUnit.Matchers;

    using EdFi.Ods.Admin.UITests.Attributes;
    using EdFi.Ods.Admin.UITests.Support.Coypu;
    using EdFi.Ods.Common.Utils.Extensions;

    using NUnit.Framework;

    [AssociatedUrl("^/Sandbox/Orphans.*$")]
    public class OrphanReportPage : PageBase
    {
        private const string RemoveButton = "#button-remove-orphans";
        private const string ConfirmCheckbox = "#checkbox-confirmoperation";
        private const string OkButton = "#btn-ok-removeall";
        private const string OrphanCount = "#orphan-count";
        private const string ModalConfirm = "#modal-remove-orphans";

        public OrphanReportPage(BrowserSession browser)
            : base(browser)
        {
        }

        public override void Visit()
        {
            if (!(this.IsCurrent()))
                this.Visit("~/Sandbox/Orphans");
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

        public void ClearOrphans()
        {
            var orphanCount = this.GetOrphanCount();
            if (orphanCount == 0)
                return;

            this.Browser.FindCss(RemoveButton).Click();
            this.Browser.FindCss(ModalConfirm).WaitForAnimation("Remove Orphans Modal", true);
            this.Browser.FindCss(ConfirmCheckbox).Click();
            this.Browser.FindCss(OkButton).Click();
            const int waitTimeSeconds = 120;
            Console.Write("Waiting {0} seconds for orphan sandboxes to be deleted.", waitTimeSeconds);
            Assert.That(this.Browser, Shows.No.Css(ModalConfirm, Make.Options.Wait(waitTimeSeconds.Seconds())), "Timed out trying to delete orphans");
        }

        public int GetOrphanCount()
        {
            var value = this.Browser.FindCss(OrphanCount).Text;
            return int.Parse(value);
        }
    }
}