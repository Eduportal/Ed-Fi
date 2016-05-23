using NDevConfig;

namespace Test.Common
{
    public class CommonTestConfiguration
    {
        public static NDevConfigSettings GetSettings()
        {
            return new NDevConfigSettings().WithProjectName("RestApi");
        }
    }
}