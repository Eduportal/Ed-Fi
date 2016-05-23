namespace EdFi.Ods.Admin.UITests.Support.Coypu
{
    using System;
    using System.Drawing.Imaging;
    using System.Text.RegularExpressions;
    using System.Threading;

    using EdFi.Ods.Common.Utils.Extensions;

    using global::Coypu;

    using OpenQA.Selenium;

    using TechTalk.SpecFlow;

    public static class BrowserSessionExtensions
    {
        private const string UserFullNameId = "userFullName";
        private const string SubmitFeedbackNameId = "submitFeedbackName";

        public static string GetLoggedInUserFullName(this BrowserSession browser)
        {
            return GetLoggedInUserFullName(browser, Make.Options.Wait(1.Seconds()));
        }

        public static string GetLoggedInUserFullName(this BrowserSession browser, Options options)
        {
            string actualUserFullName = null;

            // Try finding the user full name from hidden field on site layout first
            var fullNameElement = browser.FindField(UserFullNameId, Make.Options.Consider_Invisible_Elements.Wait(5.Seconds()));

            if (!fullNameElement.Missing())
                actualUserFullName = fullNameElement.Value;
            else
            {
                // Relies on the embedded Submit Feedback form
                var element = browser.FindField(SubmitFeedbackNameId, Make.Options.Consider_Invisible_Elements.Wait(5.Seconds()));

                if (!element.Missing())
                    actualUserFullName = element.Value;
            }

            return actualUserFullName;
        }

        /// <summary>
        /// Saves a screenshot image to the configured path (if the WebDriver implementation supports it).
        /// </summary>
        /// <returns>The path to the image file if one was saved; otherwise <b>null</b>.</returns>
        public static string SaveScreenshot(this BrowserSession browserSession, string context = null)
        {
            var screenshotter = browserSession.Driver.Native as ITakesScreenshot;

            if (screenshotter != null)
            {
                string screenshotFilename = GetScreenshotFilename(context);

                Thread.Sleep(250);
                screenshotter.GetScreenshot().SaveAsFile(screenshotFilename, ImageFormat.Png);

                return screenshotFilename;
            }

            return null;
        }

        private static string GetScreenshotFilename(string context)
        {
            string imageFilename =
                string.Format(@"{0}--{1}--{2}.png",
                              FeatureContext.Current.FeatureInfo.Title,
                              ScenarioContext.Current.ScenarioInfo.Title,
                              string.IsNullOrWhiteSpace(context) ? null : "-" + context);

            string normalizedImageFilename =
                Regex.Replace(imageFilename, @"[^\w \.-]+", string.Empty)
                     .Replace(' ', '-');

            string configuredPath = TestSessionContext.Current.Configuration.ScreenshotImagePath;
            string imageBasePath = configuredPath.EndsWith(@"\") ? configuredPath : configuredPath + @"\";

            int totalPathLength = imageBasePath.Length + normalizedImageFilename.Length;

            string outputFilename;

            if (totalPathLength >= 260)
            {
                // Adjust filename length down to fit
                int overage = totalPathLength - 260;
                string uniqueSuffix = Guid.NewGuid().GetHashCode().ToString("x");
                const string fileExtension = ".png";

                int trimLength = normalizedImageFilename.Length - overage - uniqueSuffix.Length - fileExtension.Length;

                outputFilename = string.Format("{0}{1}{2}{3}",
                                               imageBasePath,
                                               normalizedImageFilename.Substring(0, trimLength),
                                               uniqueSuffix,
                                               fileExtension);
            }
            else
            {
                outputFilename = imageBasePath + normalizedImageFilename;
            }

            return outputFilename;
        }
    }
}
