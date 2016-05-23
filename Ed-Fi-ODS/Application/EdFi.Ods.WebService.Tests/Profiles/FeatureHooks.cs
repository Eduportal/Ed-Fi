using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using Coypu;
using Coypu.Drivers;
using EdFi.Common.Extensions;
using EdFi.Ods.Api.Models.TestProfiles;
using EdFi.Ods.Api.Services.Extensions;
using EdFi.Ods.Api.Startup;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Testing;
using RestSharp;
using TechTalk.SpecFlow;
using Match = Coypu.Match;
using MetadataProfiles = EdFi.Ods.CodeGen.Models.ProfileMetadata.Profiles;

namespace EdFi.Ods.WebService.Tests.Profiles
{
    [Binding]
    public static class FeatureHooks
    {
        [BeforeFeature("SwaggerUI")]
        public static void BeforeSwaggerUIFeature()
        {
            IDisposable server = null, webApiServer = null;
            BrowserSession browser = null;

            try
            {
                // Start Swagger
                server = WebApp.Start<SwaggerUITestStartup>(SwaggerUITestStartup.BaseUrl);

                // Start the webapi
                webApiServer = WebApp.Start<ProfilesTestStartup>("http://localhost:4444/");

                // Start the browser
                string browserName = ConfigurationManager.AppSettings["WebDriver"] ?? "PhantomJS";

                if (browserName.EqualsIgnoreCase("PhantomJS"))
                    PreparePhantomJS();

                var configuration = new SessionConfiguration
                {
                    Browser = Browser.Parse(browserName),
                    Port = new Uri(SwaggerUITestStartup.BaseUrl).Port,
                    RetryInterval = browserName.EqualsIgnoreCase("Firefox") 
                        ? TimeSpan.FromMilliseconds(5000)
                        :  TimeSpan.FromMilliseconds(100),
                    Timeout = browserName.EqualsIgnoreCase("Firefox") 
                        ? TimeSpan.FromSeconds(20)
                        :  TimeSpan.FromSeconds(1),
                };

                browser = new BrowserSession(configuration);

                // When using PhantomJS, maximize window for better results
                if (configuration.Browser == Browser.PhantomJS)
                    browser.MaximiseWindow();
            }
            catch (Exception)
            {
                if (server != null)
                    server.Dispose();

                if (webApiServer != null)
                    webApiServer.Dispose();

                if (browser != null)
                    browser.Dispose();

                throw;
            }

            FeatureContext.Current.Set(server, "swaggerUIServer");
            FeatureContext.Current.Set(webApiServer, "webApiServer");
            FeatureContext.Current.Set(browser);
        }

        [BeforeFeature("SDK")]
        public static void BeforeSDKFeature()
        {
            IDisposable webApiServer = null;
            IRestClient restClient;

            try
            {
                // Start the webapi (with an HTTP endpoint)
                webApiServer = WebApp.Start<ProfilesSdkTestStartup>("http://localhost:4445/");

                // Add the RestClient
                restClient = new RestClient("http://localhost:4445/api/v2.0/2015");

                // Generate the .NET SDK
                var sdkConfig = new SdkClientGenerator.SdkConfig();
                var metadataResponse = GetMetadataResponse(sdkConfig.ApiMetadataUrl);
                var metadataETag = metadataResponse.Headers.Get("ETag").Unquoted();

                string programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

                // This should never happen when using a SpecialFolder entry.
                if (string.IsNullOrEmpty(programDataPath))
                    throw new Exception("Environment.GetFolderPath for special folder 'CommonApplicationData' did not return a value.  The .NET SDK cannot be generated.");

                string sdkGenerationBasePath = Path.Combine(programDataPath, sdkConfig.SdkProjectName);

                try
                {
                    // Ensure the SDK code generation base path exists
                    Directory.CreateDirectory(sdkGenerationBasePath);
                }
                catch (Exception ex)
                {
                    // Throw a helpful message if something goes awry.
                    string message = string.Format(
                        "An unexpected exception occurred while trying to create the '{0}' directory for .NET SDK code generation.",
                        sdkGenerationBasePath);

                    throw new Exception(message, ex);
                }

                var tempMetadataETagPath = Path.Combine(sdkGenerationBasePath, metadataETag);
                var tempSdkAssemblyPath = tempMetadataETagPath + @"\" + sdkConfig.SdkProjectName + @"\"+ sdkConfig.SdkAssemblyName;

                var filteredMetadataSections = GetFilteredSections(metadataResponse);
                SdkClientGenerator.EnsureClientSdkSourceCodeGenerated(sdkConfig.ApiMetadataUrl, filteredMetadataSections, tempMetadataETagPath, sdkConfig);
                SdkClientGenerator.EnsureDotNetSdkAssemblyCompiled(tempSdkAssemblyPath);

                FeatureContext.Current.Set(tempSdkAssemblyPath, "SdkAssemblyPath");
            }
            catch (Exception)
            {
                if (webApiServer != null)
                    webApiServer.Dispose();

                throw;
            }

            FeatureContext.Current.Set(webApiServer, "webApiServer");
            FeatureContext.Current.Set(restClient, "restClient");
        }

        private static HttpWebResponse GetMetadataResponse(string apiMetadataUrl)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(apiMetadataUrl);
            webRequest.Method = WebRequestMethods.Http.Get;
            var webResponse = (HttpWebResponse)webRequest.GetResponse();

            return webResponse;
        }

        private static List<string> GetFilteredSections(HttpWebResponse webResponse)
        {
            var excludeList = new List<string>
            {
                "Descriptors",
                "Types",
                "Resources",
                "Other"
            };

            var streamReader = new StreamReader(webResponse.GetResponseStream(), true);
            var responseContent = streamReader.ReadToEnd();

            var metadataSections = Regex.Matches(responseContent, @"(?<=""value"":"")([\w\-]+)")
                        .Cast<System.Text.RegularExpressions.Match>()
                        .Select(match => match.Value)
                        .ToList();

            streamReader.Close();
            webResponse.Close();

            return metadataSections.Where(section =>
                                            excludeList.All(exclude =>
                                                !section.EqualsIgnoreCase(exclude)))
                                                .ToList();
        }

        private static void PreparePhantomJS()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string directoryForEnvironmentPathVariable = currentDirectory;

            // If we're running test within NCrunch
            if (currentDirectory.Contains("NCrunch"))
            {
                // Copy PhantomJS.exe to a stable path location (to avoid repeated Windows Firewall popups)
                string phantomJSSource = Path.Combine(
                    currentDirectory,
                    "PhantomJS.exe");

                string phantomJSTarget = Path.Combine(
                    Environment.GetEnvironmentVariable("ProgramData"),
                    "EdFi",
                    "PhantomJS.exe");

                if (File.Exists(phantomJSSource))
                {
                    bool shouldCopy = true;

                    if (File.Exists(phantomJSTarget))
                    {
                        // Check file date before copying
                        var sourceInfo = new FileInfo(phantomJSSource);
                        var targetInfo = new FileInfo(phantomJSTarget);

                        if (sourceInfo.LastWriteTime == targetInfo.LastWriteTime)
                            shouldCopy = false;
                    }

                    if (shouldCopy)
                        File.Copy(phantomJSSource, phantomJSTarget, true);

                    if (File.Exists(phantomJSTarget))
                        directoryForEnvironmentPathVariable = Path.GetDirectoryName(phantomJSTarget);
                }
            }

            // Modify the current process' PATH environment variable to include the folder containing the PhantomJS.exe file.
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            if (path != null && !path.Contains(directoryForEnvironmentPathVariable))
            {
                path = directoryForEnvironmentPathVariable + ";" + path;
                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
            }
        }

        [AfterFeature("SwaggerUI")]
        public static void AfterSwaggerUIFeature()
        {
            var browser = FeatureContext.Current.Get<BrowserSession>();
            browser.Dispose();

            var server = FeatureContext.Current.Get<IDisposable>("swaggerUIServer");
            server.Dispose();

            var apiServer = FeatureContext.Current.Get<IDisposable>("webApiServer");
            apiServer.Dispose();
        }

        [AfterFeature("SDK")]
        public static void AfterSDKFeature()
        {
            var apiServer = FeatureContext.Current.Get<IDisposable>("webApiServer");
            apiServer.Dispose();
        }

        [BeforeFeature]
        public static void BeforeFeature()
        {
            var serializer = new XmlSerializer(typeof(MetadataProfiles));
            var stream = typeof(Marker_EdFi_Ods_Api_Models_TestProfiles).Assembly
                .GetManifestResourceStream("EdFi.Ods.Api.Models.TestProfiles.Profiles.xml");

            var sr = new StreamReader(stream);
            string xml = sr.ReadToEnd();

            FeatureContext.Current.Set(XDocument.Parse(xml), "ProfilesXDocument");

            var profiles = (MetadataProfiles)serializer.Deserialize(new StringReader(xml));
            FeatureContext.Current.Set(profiles);
        }

        [BeforeFeature("API")]
        public static void BeforeApiFeature()
        {
            var startup = new ProfilesTestStartup();
            var server = TestServer.Create(startup.Configuration);
            FeatureContext.Current.Set(server);
            FeatureContext.Current.Set(startup.InternalContainer);
            FeatureContext.Current.Set(startup, "OWINstartup");

            var client = new HttpClient(server.Handler);
            FeatureContext.Current.Set(client);

            client.Timeout = new TimeSpan(0, 0, 15, 0);

            // Set client's authorization header to an arbitrary value
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());
        }

        [AfterFeature("API")]
        public static void AfterFeature()
        {
            FeatureContext.Current.Get<StartupBase>("OWINstartup").Dispose();
            FeatureContext.Current.Get<TestServer>().Dispose();
            FeatureContext.Current.Get<HttpClient>().Dispose();
        }
    }
}