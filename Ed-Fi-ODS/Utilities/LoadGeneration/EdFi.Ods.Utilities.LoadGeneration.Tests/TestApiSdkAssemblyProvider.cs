using System.Reflection;

namespace EdFi.Ods.Utilities.LoadGeneration.Tests
{
    public class TestApiSdkAssemblyProvider : IApiSdkAssemblyProvider
    {
        public Assembly GetAssembly()
        {
            return typeof(TestApiSdkAssemblyProvider).Assembly;
        }
    }
}