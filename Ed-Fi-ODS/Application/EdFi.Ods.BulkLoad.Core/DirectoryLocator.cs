using System.IO;
using EdFi.Common.Configuration;
using EdFi.Ods.Common;

namespace EdFi.Ods.BulkLoad.Core
{
    public class DirectoryLocator : IDirectoryLocator
    {

        private string root;

        public DirectoryLocator(IConfigValueProvider configValueProvider)
        {
            root = configValueProvider.GetValue("RootXmlDirectory");
        }

        public string GetNamedDirectory(string directoryName)
        {
            if (!Directory.Exists(root)) root = @"C:\temp";
            var directory = root + @"\" + directoryName;
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            return directory;
        }
    }
}