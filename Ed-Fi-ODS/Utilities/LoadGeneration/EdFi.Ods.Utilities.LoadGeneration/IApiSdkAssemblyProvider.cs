using System.IO;
using System.Reflection;
using EdFi.Ods.Utilities.LoadGeneration.Security;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface IApiSdkAssemblyProvider
    {
        Assembly GetAssembly();
    }

    public class FilePathApiSdkAssemblyProvider : IApiSdkAssemblyProvider
    {
        private readonly IApiSecurityContextProvider _apiSecurityContextProvider;

        public FilePathApiSdkAssemblyProvider(IApiSecurityContextProvider apiSecurityContextProvider)
        {
            _apiSecurityContextProvider = apiSecurityContextProvider;
        }

        private Assembly _assembly;

        public Assembly GetAssembly()
        {
            if (_assembly == null)
                _assembly = LoadAssembly();

            return _assembly;
        }

        private Assembly LoadAssembly()
        {
            var context = _apiSecurityContextProvider.GetSecurityContext();

            string path;

            if (!Path.IsPathRooted(context.ApiSdkAssemblyPath))
            {
                path = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    context.ApiSdkAssemblyPath);
            }
            else
            {
                path = context.ApiSdkAssemblyPath;
            }

            var fileInfo = new FileInfo(path);

            if (!fileInfo.Exists)
                throw new FileNotFoundException(string.Format("The REST API SDK assembly could not be found at '{0}'.",
                    path));

            var assembly = Assembly.LoadFile(path);

            return assembly;
        }
    }
}
