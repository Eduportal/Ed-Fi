using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Castle.Windsor;
using Coypu;
using EdFi.TestObjects;
using TechTalk.SpecFlow;

namespace EdFi.Ods.WebService.Tests.Profiles
{
    [Binding]
    public sealed class ScenarioHooks
    {
        [BeforeScenario]
        public void BeforeScenario()
        {
            IWindsorContainer container;

            // Reset the TestObjectFactory values between scenarios, when relevant
            if (FeatureContext.Current.TryGetValue(out container))
            {
                var testObjectFactory = container.Resolve<ITestObjectFactory>();
                testObjectFactory.ResetValueBuilders();
            }
        }

        // For additional details on SpecFlow hooks see http://go.specflow.org/doc-hooks
        [AfterScenario("SwaggerUI")]
        public void AfterScenario()
        {
            var browser = FeatureContext.Current.Get<BrowserSession>();
            
            string filename = Path.Combine(
                Environment.GetEnvironmentVariable("ProgramData"),
                "EdFi",
                "Screenshots",
                "SwaggerUI",
                ScenarioContext.Current.ScenarioInfo.Title
                    + (ScenarioContext.Current.TestError != null ? "(Error)" : string.Empty)
                    + ".png");

            if (!Directory.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename));

            browser.SaveScreenshot(filename, ImageFormat.Png);

            Console.WriteLine("View UI testing screenshots in '{0}'.", Path.GetDirectoryName(filename));
        }
    }
}
