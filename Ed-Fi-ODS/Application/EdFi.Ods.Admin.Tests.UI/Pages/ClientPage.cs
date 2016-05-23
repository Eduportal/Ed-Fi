namespace EdFi.Ods.Admin.UITests.Pages
{
    using System.Linq;

    using Coypu;

    using EdFi.Ods.Admin.UITests.Attributes;
    using EdFi.Ods.Admin.UITests.Support.Coypu;

    using Should;

    [AssociatedUrl("^/Account/ActivateAccount.*$")]
    public class ClientPage : PageBase
    {
        public ClientPage(BrowserSession browser) : base(browser)
        {
        }

        public override void Visit()
        {
            if (!(this.IsCurrent()))
                this.Visit("~/Client");
        }

        public void CreateNewApplication(string name, bool withSampleData = false)
        {
            this.ClickAddApplication();
            this.EnterName(name);
            if (withSampleData)
                this.ClickUseSampleData();
            this.ClickAdd();
        }

        private void ClickAddApplication()
        {
            this.Browser.FindCss("#add-application").Click();
        }

        private void EnterName(string name)
        {
            this.Browser.FindId("txtName").FillInWith(name);
        }

        private void ClickUseSampleData()
        {
            this.Browser.FindCss("#checkbox-usesampledata").Click();
        }

        private void ClickAdd()
        {
            this.Browser.FindCss("#btn-ok-add").Click(); //No Idea if this works. :P
        }

        public bool SampleDataIsInUse(string name)
        {
            var hasSampleData = this.GetApplicationRow(name).FindCss(".application-has-sample-data").Text;
            return (hasSampleData.ToLower().Trim() == "yes");
        }

        public bool IsApplicationInTable(string name)
        {
            return this.Browser.HasContent(name);
        }

        private string BuildRowSelectCss(int rowNumber, string additionalCss = "")
        {
            additionalCss = additionalCss.TrimStart();
            var css = string.Format("#clients-table > tbody > tr:nth-child({0}) {1}", rowNumber, additionalCss).Trim();
            return css;
        }

        private int GetRowNumber(string name)
        {
            var rows = this.Browser.FindAllCss("#clients-table > tbody > tr");
            int rowcount = rows.Count();
            for (int rowNumber = 1; rowNumber <= rowcount; rowNumber++)
            {
                string namelocator = this.BuildRowSelectCss(rowNumber, "> .client-name");
                if (this.Browser.FindCss(namelocator).Text == name)
                {
                    return rowNumber;
                }
            }
            return 1;
        }

        private ElementScope GetApplicationRow(string name)
        {
            var selector = string.Format("#clients-table tr[data-applicationname=\"{0}\"]", name);
            var applicationRow = this.Browser.FindCss(selector);
            applicationRow.Exists()
                          .ShouldBeTrue(string.Format("Could not find application row for application named '{0}'", name));
            return applicationRow;
        }

        private ElementScope GetApplicationOptions(ElementScope applicationRow)
        {
            var button = applicationRow.FindCss(".actionButton");
            button.Exists().ShouldBeTrue("Could not locate application options button");
            return button;
        }

        public void DeleteSandbox(string name)
        {
            var applicationRow = this.GetApplicationRow(name);
            applicationRow.FindCss(".actionButton").Click();
            applicationRow.FindCss(".deleteApplication").Click();
            this.ConfirmOperation();
        }

        private void ConfirmOperation()
        {
            this.Browser.FindCss("#checkbox-confirmoperation")
                   .WaitForAnimation("confirmation checkbox")
                   .Click();

            var okButton = this.Browser.FindCss("#btn-ok-confirm");
            okButton.Exists().ShouldBeTrue("Could not find the confirmation button");
            okButton.Click();
        }

        public void UpdateKeys(string name)
        {
            var applicationRow = this.GetApplicationRow(name);
            applicationRow.FindCss(".actionButton").Click();
            applicationRow.FindCss(".changeSecret").Click();
            this.ConfirmOperation();
        }

        public KeyPair getKeyandSecret(string name)
        {
            int rowNumber = this.GetRowNumber(name);
            string secretSelector = this.BuildRowSelectCss(rowNumber, "> .application-secret");
            string keySelector = this.BuildRowSelectCss(rowNumber, "> .application-key");

            return new KeyPair(this.Browser.FindCss(secretSelector).Text, this.Browser.FindCss(keySelector).Text);
        }
    }

    public class KeyPair
    {
        public string Key { get; private set; }
        public string Secret { get; private set; }

        public KeyPair(string secret, string key)
        {
            this.Key = key;
            this.Secret = secret;
        }
    }
}