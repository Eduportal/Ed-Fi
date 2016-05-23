namespace EdFi.Ods.Admin.UITests.Support
{
    using System;

    using global::Coypu;
    using global::Coypu.Drivers;

    public class TestConfiguration
    {
        /// <summary>
        ///     Gets or sets the browser type to use for the testing.
        /// </summary>
        public Browser BrowserType { get; set; }

        /// <summary>
        ///     Gets or sets the browser timeout period in seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        ///     Gets the fully constructed base URL for the website to be tested (return value contains trailing forward slash).
        /// </summary>
        public string ServerBaseUrl { get; set; }

        public string ScreenshotImagePath { get; set; }

        public string LocalEmailDirectory { get; set; }

        public bool BrowserMaximized { get; set; }

        public SessionConfiguration SessionConfiguration
        {
            get { return new SessionConfiguration {Browser = BrowserType, Timeout = TimeSpan.FromSeconds(TimeoutSeconds)}; }
        }

        public SmtpMode SmtpMode { get; set; }

        public string AdminAppPath { get; set; }

        public int Port
        {
            get { return new UriBuilder(ServerBaseUrl).Port; }
        }
    }
}