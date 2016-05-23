namespace EdFi.Ods.Admin.UITests.Support.SpecFlow
{
    using System;

    using BoDi;

    using EdFi.Ods.Admin.UITests.Support.Account;

    using TechTalk.SpecFlow;

    [Binding]
    public class EmailBinding
    {
        private readonly IObjectContainer objectContainer;

        public EmailBinding(IObjectContainer objectContainer)
        {
            this.objectContainer = objectContainer;
        }

        [BeforeScenario]
        public void InitializeEmailReader()
        {
            var testSessionContext = TestSessionContext.Current;
            var smtpMode = testSessionContext.Configuration.SmtpMode;

            switch (smtpMode)
            {
                    //DANGER WILL ROBINSON!!!  -MUST MATCH SMTP VALUES IN ADMIN WEB CONFIG - DANGER!!!
                case SmtpMode.File:
                    this.objectContainer.RegisterInstanceAs<IEmailReader>(new FileSystemEmailReader(testSessionContext));
                    break;
                case SmtpMode.Network:
                    this.objectContainer.RegisterInstanceAs<IEmailReader>(new GMailReader());
                    break;
                default:
                    throw new NotSupportedException(string.Format("Define an email reader for SmtpMode.{0}", smtpMode.ToString()));
            }
        }
    }
}
