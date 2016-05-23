//#if DEBUG
namespace EdFi.Ods.Admin.UITests.Support.SpecFlow
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using TechTalk.SpecFlow;

    using Test.Common;

    [Binding]
    // ReSharper disable once InconsistentNaming
    public class IISExpressBinding
    {
        private static Process iisProcess;

        [BeforeTestRunAttribute]
        public static void Before()
        {
            var path = Path.Combine(
                // ReSharper disable once AssignNullToNotNullAttribute
                Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(
                Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory))))),
                TestSessionContext.Current.Configuration.AdminAppPath);
            var port = TestSessionContext.Current.Configuration.Port;
            iisProcess = IISExpressHelper.Start(path, port);
        }

        [AfterTestRun]
        public static void After()
        {
            IISExpressHelper.Stop(iisProcess);
        }
    }
}
//#endif
