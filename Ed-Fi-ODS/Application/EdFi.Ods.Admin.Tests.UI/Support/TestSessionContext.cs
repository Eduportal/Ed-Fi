namespace EdFi.Ods.Admin.UITests.Support
{
    using System;
    using System.IO;
    using System.Reflection;

    using EdFi.Ods.Admin.UITests.Support.Configuration;

    using global::Coypu.Drivers;

    using NDevConfig;

    using Test.Common;

    /// <summary>
    ///     Provides context (user profiles, configuration values, etc.) for a test session.
    /// </summary>
    public class TestSessionContext
    {
        private static TestSessionContext _testSessionContext;

        /// <summary>
        ///     Gets the current context for the executing test session.
        /// </summary>
        public static TestSessionContext Current
        {
            get
            {
                if (_testSessionContext == null)
                    _testSessionContext = new TestSessionContext();

                return _testSessionContext;
            }
        }

        #region Configuration

        /// <summary>
        ///     Holds the value for the <see cref="Configuration" /> property.
        /// </summary>
        private TestConfiguration _configuration;

        private NDevConfiguration _nDevConfig;

        private NDevConfiguration NDevConfig
        {
            get
            {
                if (this._nDevConfig == null)
                    this._nDevConfig = new NDevConfiguration(CommonTestConfiguration.GetSettings());
                return this._nDevConfig;
            }
        }

        /// <summary>
        ///     Gets the configuration values for the test session.
        /// </summary>
        public TestConfiguration Configuration
        {
            get
            {
                if (this._configuration == null)
                {
                    var adminAppPath = this.NDevConfig.GetSetting(ConfigurationKeys.AdminAppPath);
                    var serverBaseUrl = this.NDevConfig.GetSetting(ConfigurationKeys.ServerBaseUrl);
                    var timeoutSecondsSetting = this.NDevConfig.GetOptionalIntSetting(ConfigurationKeys.TimeoutSeconds);
                    var timeoutSeconds = timeoutSecondsSetting.HasValue ? timeoutSecondsSetting.Value : 20;
                    var screenshotImagePath = this.NDevConfig.GetSetting(ConfigurationKeys.ScreenshotImagePath);
                    var smtpMode = this.NDevConfig.GetSetting(ConfigurationKeys.SmtpMode);
                    var maximizeWindow = this.NDevConfig.GetOptionalBoolSetting(ConfigurationKeys.BrowserMaximized);

                    var smtpModeParts = smtpMode.Split('-');
                    var localEmailDirectory = smtpModeParts.Length == 1 ? string.Empty : smtpModeParts[1];

                    var browserType = GetBrowserType(this.NDevConfig.GetSetting(ConfigurationKeys.BrowserType));

                    // Expand environment variables in paths
                    if (adminAppPath != null)
                        adminAppPath = Environment.ExpandEnvironmentVariables(adminAppPath);

                    if (screenshotImagePath != null)
                        screenshotImagePath = Environment.ExpandEnvironmentVariables(screenshotImagePath);

                    if (localEmailDirectory != null)
                        localEmailDirectory = Environment.ExpandEnvironmentVariables(localEmailDirectory);


                    // If paths are still relative, base them off the current user's Temp path
                    screenshotImagePath = EnsureRelativePathsBasedOnTempPath(screenshotImagePath);
                    localEmailDirectory = EnsureRelativePathsBasedOnTempPath(localEmailDirectory);

                    this._configuration = new TestConfiguration
                                         {
                                             AdminAppPath = adminAppPath,
                                             ServerBaseUrl = serverBaseUrl,
                                             TimeoutSeconds = timeoutSeconds,
                                             ScreenshotImagePath = screenshotImagePath,
                                             SmtpMode = (SmtpMode) Enum.Parse(typeof(SmtpMode), smtpModeParts[0], true),
                                             LocalEmailDirectory = localEmailDirectory,
                                             BrowserMaximized = maximizeWindow,
                                             BrowserType = browserType,
                                         };

                    // Set the browser type
                }

                return this._configuration;
            }
        }

        private static string EnsureRelativePathsBasedOnTempPath(string path)
        {
            // Is the screenshot path a relative path?
            if (!Path.IsPathRooted(path))
            {
                // Prepend the "TEMP" folder setting
                path = Path.Combine(Path.GetTempPath(), path);
            }
            
            return path;
        }

        private static Browser GetBrowserType(string browserType)
        {
            FieldInfo browserTypeField = typeof(Browser).GetField(browserType,
                                                                   BindingFlags.Public | BindingFlags.Static);

            if (browserTypeField != null)
                return browserTypeField.GetValue(null) as Browser;

            // Default to Firefox (it's the most well-behaved)
            return Browser.Firefox;
        }

        #endregion
    }

    public enum SmtpMode
    {
        File,
        Network,
    }
}