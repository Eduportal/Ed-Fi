namespace EdFi.Ods.Admin.UITests.Steps
{
    using System;
    using System.IO;
    using System.Net;

    using BoDi;

    using Coypu;

    using EdFi.Common.Configuration;
    using EdFi.Ods.Admin.Models;
    using EdFi.Ods.Admin.UITests.Pages;
    using EdFi.Ods.Admin.UITests.Support;
    using EdFi.Ods.Admin.UITests.Support.Coypu;
    using EdFi.Ods.Admin.UITests.Support.Extensions;
    using EdFi.Ods.Admin.UITests.Support.Navigation;

    using TechTalk.SpecFlow;

    [Binding]
    public class Hooks
    {
        // For additional details on SpecFlow hooks see http://go.specflow.org/doc-hooks

        private readonly IObjectContainer _container;
        private DebugMarker _debugMode;
        private BrowserSession _browser;
        private TestConfiguration _configuration;

        public Hooks(IObjectContainer container)
        {
            this._container = container;
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            PageRegistry.Initialize();

            // Ignore SSL errors on out-of-band HTTP-based calls
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, sslPolicyErrors) => true;

            InitializeArtifactFolders();
        }

        private static void InitializeArtifactFolders()
        {
            // Ensure folders exist, and are empty
            InitializeArtifactFolder(TestSessionContext.Current.Configuration.ScreenshotImagePath);
            InitializeArtifactFolder(TestSessionContext.Current.Configuration.LocalEmailDirectory);

            Console.WriteLine(
                @"------------------------------------------------------------------------
Paths initialized for artifacts:
    Screenshots: {0}
    Emails:      {1}
------------------------------------------------------------------------",
                TestSessionContext.Current.Configuration.ScreenshotImagePath, 
                TestSessionContext.Current.Configuration.LocalEmailDirectory);
        }

        private static void InitializeArtifactFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                // Create the folder
                Directory.CreateDirectory(path);
            }
            else
            {
                // Clear the existing folder
                var directoryInfo = new DirectoryInfo(path);
                directoryInfo.DeleteFiles("*.png", "*.eml");
            }
        }

        [BeforeFeature]
        public static void BeforeFeature()
        {
            FeatureContext.Current.ResetUserAccountCache();
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            this._debugMode = new DebugMarker();
            this._configuration = TestSessionContext.Current.Configuration;
            this._browser = new BrowserSession(this._configuration.SessionConfiguration);

            if (this._configuration.BrowserMaximized)
                this._browser.MaximiseWindow();

            this.InitObjectContainer();

            this._container.Resolve<HomePage>().Visit();
        }

        [BeforeScenario("debug")]
        public void SetDebugMode()
        {
            this._debugMode.SetDebug();
        }

        [AfterScenario]
        public void AfterScenario()
        {
            if (ScenarioContext.Current.TestError != null)
                this._browser.SaveScreenshot();

            if (this._debugMode.IsDebug)
                this._debugMode.Fail();
            else
                this._browser.Dispose();
        }

        private void InitObjectContainer()
        {
            this._container.RegisterInstanceAs(
                Activator.CreateInstance(
                    SandboxProvisionerTypeCalculator.GetSandboxProvisionerTypeForNewSandboxes()) as ISandboxProvisioner);
            this._container.RegisterInstanceAs(new UsersContext());
            this._container.RegisterTypeAs<ClientAppRepo, IClientAppRepo>();
            this._container.RegisterTypeAs<AppConfigValueProvider, IConfigValueProvider>();

            this._container.RegisterInstanceAs(this._browser);
            this._container.RegisterInstanceAs(this._debugMode);
        }
    }
}