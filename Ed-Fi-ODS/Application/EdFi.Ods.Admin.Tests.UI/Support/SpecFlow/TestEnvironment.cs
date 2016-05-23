namespace EdFi.Ods.Admin.UITests.Support.SpecFlow
{
    using EdFi.Ods.Admin.Models;

    using TechTalk.SpecFlow;

    using Test.Common;

    using TestUserInitializer = EdFi.Ods.Admin.UITests.Support.Account.TestUserInitializer;

    [Binding]
    public class TestEnvironment
    {
        [BeforeTestRunAttribute]
        public static void Before()
        {
            TestUserInitializer.Initialize(new UsersContext());
        }

        [AfterTestRunAttribute]
        public static void After()
        {
            
        }
    }
}
