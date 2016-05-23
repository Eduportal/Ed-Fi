namespace EdFi.Ods.XsdParsing.Tests
{
    using System;
    using System.IO;

    using NDevConfig;

    using Test.Common;

    public class TestConfiguration
    {
        private readonly NDevConfiguration _nDevConfig = new NDevConfiguration(CommonTestConfiguration.GetSettings());

        public string InterchangePath
        {
            get
            {
                var repoRoot = this._nDevConfig.GetSetting("RepositoryRoot");
                if (!Directory.Exists(repoRoot))
                    throw new Exception(string.Format("Configured 'RepositoryRoot' with value '{0}' does not exist", repoRoot));

                var relativePath = this._nDevConfig.GetSetting("RelativeInterchangePath");
                var path = Path.Combine(repoRoot, relativePath);
                if (!Directory.Exists(path))
                    throw new Exception(string.Format("Configured 'InterchangePath' with value '{0}' does not exist", path));
                return path;
            }
        }
    }
}