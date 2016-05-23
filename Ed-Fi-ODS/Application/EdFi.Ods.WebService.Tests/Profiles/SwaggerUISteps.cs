using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Coypu;
using EdFi.Common.Extensions;
using EdFi.Common.Inflection;
using EdFi.Ods.CodeGen.Models.ProfileMetadata;
using EdFi.Ods.Common.Utils.Profiles;
using NUnit.Framework;
using Should;
using TechTalk.SpecFlow;
using MetadataProfiles = EdFi.Ods.CodeGen.Models.ProfileMetadata.Profiles;

namespace EdFi.Ods.WebService.Tests.Profiles
{
    [Binding]
    public sealed class SwaggerUISteps
    {
        // For additional details on SpecFlow step definitions see http://go.specflow.org/doc-stepdef

        [Given(@"I have loaded a fresh copy of the SwaggerUI web page in a web browser")]
        public void Given_I_have_opened_the_SwaggerUI_web_page_in_a_web_browser()
        {
            var browser = FeatureContext.Current.Get<BrowserSession>();
            browser.Visit(SwaggerUITestStartup.BaseUrl);
            WaitForInitialSectionToRender(browser);
        }

        [Given(@"I am on the SwaggerUI web page in a web browser")]
        public void GivenIAmOnTheSwaggerUIWebPageInAWebBrowser()
        {
            var browser = FeatureContext.Current.Get<BrowserSession>();

            if (browser.Location != new Uri(SwaggerUITestStartup.BaseUrl))
            {
                browser.Visit(SwaggerUITestStartup.BaseUrl);
                WaitForInitialSectionToRender(browser);
            }
        }

        [When(@"I select the API section ""(.*)""")]
        public void GivenISelectTheAPISection(string sectionName)
        {
            var browser = FeatureContext.Current.Get<BrowserSession>();
            browser.FindId("select_baseUrl").SelectOption(sectionName, new Options { Timeout = TimeSpan.FromSeconds(10) });
        }

        [When(@"I select the entry for the profile named ""(.*)""")]
        public void WhenISelectTheEntryForTheProfileNamed(string profileName)
        {
            var browser = FeatureContext.Current.Get<BrowserSession>();
            
            browser.FindId("select_baseUrl").SelectOption(profileName, new Options { WaitBeforeClick = TimeSpan.FromSeconds(2), Timeout = TimeSpan.FromSeconds(10) });

            var profiles = FeatureContext.Current.Get<MetadataProfiles>();
            Profile profile = profiles.Profile.SingleOrDefault(x => x.name == profileName);

            if (profile == null)
                throw new Exception(string.Format("Profile '{0}' could not be found.", profileName));

            ScenarioContext.Current.Set(profileName, "profileName");
            ScenarioContext.Current.Set(profile);
        }

        private static void WaitForInitialSectionToRender(BrowserSession browser)
        {
            browser.TryUntil(
                () => { Console.WriteLine("Waiting for > 10 resources on web page..."); },
                () => browser.FindAllCss("li.resource").Count() > 10,
                TimeSpan.FromSeconds(2),
                new Options {Timeout = TimeSpan.FromSeconds(30), RetryInterval = TimeSpan.FromSeconds(2)});
        }

        [Then(@"the (first|second|third|fourth|fifth) entry in the API section list should be ""(.*)""")]
        public void Then_the__first_second_third_fourth__entry_in_the_API_section_list_should_be_________(string entryInstance, string text)
        {
            int entryIndex = -1;

            switch (entryInstance)
            {
                case "first":
                    entryIndex = 0;
                    break;
                case "second":
                    entryIndex = 1;
                    break;
                case "third":
                    entryIndex = 2;
                    break;
                case "fourth":
                    entryIndex = 3;
                    break;
                case "fifth":
                    entryIndex = 4;
                    break;
            }

            var browser = FeatureContext.Current.Get<BrowserSession>();
            var options = GetApiSectionOptions(browser);

            string allOptionsMessageText = GetAllOptionsMessageText(options);

            options
                .Where((o, i) => 
                    o.Text.EqualsIgnoreCase(text) 
                    && i == entryIndex).Any()
                .ShouldBeTrue(
                    string.Format("Unable to find {0} at position {1} in the following options found in the API section drop down list:\r\n{2}", 
                    text, entryIndex, allOptionsMessageText));
        }

        private static string GetAllOptionsMessageText(ElementScope[] options)
        {
            string allOptions = string.Join("\n", options.Select((x, i) => string.Format("[{0}] {1}", i, x.Text)));
            return allOptions;
        }

        private static ElementScope[] GetApiSectionOptions(BrowserSession browser)
        {
            ElementScope[] options;

            if (!ScenarioContext.Current.TryGetValue("apiSectionOptions", out options))
            {
                options = (ElementScope[]) browser
                    .FindAllXPath("//select[@id='select_baseUrl']/option",
                    x => x.Count() > 1,
                    new Options
                    {
                        RetryInterval = TimeSpan.FromSeconds(1),
                        Timeout = TimeSpan.FromSeconds(30)
                    });

                ScenarioContext.Current.Set(options, "apiSectionOptions");
            }
            return options;
        }

        [Then(@"each profile entry in the API section list should use the profile name as the value")]
        public void Then_each_profile_entry_in_the_API_section_list_should_use_the_profile_name_as_the_value()
        {
            var browser = FeatureContext.Current.Get<BrowserSession>();
            var options = GetApiSectionOptions(browser);

            var profiles = FeatureContext.Current.Get<MetadataProfiles>();

            // Each profile should be present in the list
            foreach (Profile profile in profiles.Profile)
            {
                options.Any(x => 
                    x.Value.EqualsIgnoreCase(profile.name))
                    .ShouldBeTrue();
            }
        }

        [Then(@"each profile entry in the API section list should be displayed with a prefix of ""(.*)""")]
        public void Then_each_profile_entry_in_the_API_section_list_should_be_prefixed_with_________(string prefix)
        {
            var browser = FeatureContext.Current.Get<BrowserSession>();
            var options = GetApiSectionOptions(browser);

            string allOptionsMessageText = GetAllOptionsMessageText(options);

            var profiles = FeatureContext.Current.Get<MetadataProfiles>();

            // Each profile should be present in the list
            foreach (Profile profile in profiles.Profile)
            {
                string expectedDisplayName = prefix + profile.name.NormalizeCompositeTermForDisplay('-');

                options.Any(x => x.Text.EqualsIgnoreCase(expectedDisplayName))
                    .ShouldBeTrue(string.Format("Unable to find entry for profile '{0}' (using expected display text of '{1}') in the following list:\r\n{2}.", 
                        profile.name, expectedDisplayName, allOptionsMessageText));
            }
        }

        [Then(@"the number of entries in the API section list should correspond to the number of profiles plus the (.*) standard sections")]
        public void Then_the_number_of_entries_in_the_API_section_list_should_correspond_to_the_number_of_profiles_plus_the______standard_sections(int standardSectionCount)
        {
            var browser = FeatureContext.Current.Get<BrowserSession>();
            var options = GetApiSectionOptions(browser);

            string allOptionsMessageText = GetAllOptionsMessageText(options);

            var profiles = FeatureContext.Current.Get<MetadataProfiles>();

            int expectedCount = profiles.Profile.Length + standardSectionCount;

            expectedCount
                .ShouldEqual(options.Length, string.Format("The number of items found in the API sections drop down list ({0}) did not match the expected count of {1}.  Entries found were: {2}",
                options.Length, expectedCount, allOptionsMessageText));
        }

        [Then(@"""(.*)"" should be selected in the API sections drop down list")]
        public void Then_a_value_should_be_selected_in_the_API_sections_drop_down_list(string entryValue)
        {
            var browser = FeatureContext.Current.Get<BrowserSession>();

            Assert.That(browser.FindId("select_baseUrl").SelectedOption, Is.EqualTo(entryValue));
        }

        [Then(@"the page title should (not )?contain ""(.*)""")]
        public void ThenThePageTitleShouldContain(string not, string substring)
        {
            var browser = FeatureContext.Current.Get<BrowserSession>();

            var titleElement = browser.FindCss(".info_title");

            if (string.IsNullOrEmpty(not))
            {
                if (!titleElement.HasContent(substring, new Options { Timeout = TimeSpan.FromSeconds(20) }))
                    Assert.That(titleElement.Text, Is.StringContaining(substring));
            }
            else
            {
                if (!titleElement.HasNoContent(substring, new Options { Timeout = TimeSpan.FromSeconds(20) }))
                    Assert.That(titleElement.Text, Is.Not.StringContaining(substring));
            }
        }

        [Then(@"the page should only contain sections for the resources defined in the profile")]
        public void ThenThePageShouldOnlyContainSectionsForTheResourceInTheProfile()
        {
            var browser = FeatureContext.Current.Get<BrowserSession>();
            var profile = ScenarioContext.Current.Get<Profile>();

            var actualSectionNames = GetResourceHeaderLinks(browser)
                .Select(x => x.Text);

            var expectedSectionNames = profile.Resource.Select(r => 
                CompositeTermInflector.MakePlural(r.name).ToCamelCase());

            Assert.That(actualSectionNames, Is.EquivalentTo(expectedSectionNames));
        }

        private static IEnumerable<SnapshotElementScope> GetResourceHeaderLinks(BrowserSession browser)
        {
            return browser.FindAllCss("li.resource div.heading h2 a");
        }

        private static IEnumerable<SnapshotElementScope> GetResourceItems(BrowserSession browser)
        {
            return browser.FindAllCss("li.resource");
        }

        [When(@"I expand all the resource sections")]
        public void WhenIExpandAllTheResourceSections()
        {
            var browser = FeatureContext.Current.Get<BrowserSession>();

            try
            {
                // Prevent any chance of an infinite loop
                var sw = new Stopwatch();
                sw.Start();

                // Try for up to 30 seconds
                while (sw.Elapsed < TimeSpan.FromSeconds(60))
                {
                    // Expand the resource sections of inactive resource items
                    var resourceLink = browser.FindCss("li.resource:not(.active) div.heading h2 a", new Options { Match = Match.First });
                    resourceLink.Click();
                }
            }
            catch (MissingHtmlException)
            {
                // Expected when there are no more inactive resources
            }
        }

        [Then(@"the GET operations should only provide a single option for the Response Content Type that is the profile-specific content type for the resource")]
        public void ThenTheGETOperationsShouldOnlyProvideASingleOptionForTheResponseContentTypeThatIsTheProfile_SpecificContentTypeForTheResource()
        {
            VerifyAvailableContentTypes(HttpMethod.Get);
        }

        [Then(@"the PUT operations should only provide a single option for the Parameter Content Type that is the profile-specific content type for the resource")]
        public void ThenThePUTOperationsShouldOnlyProvideASingleOptionForTheParameterContentTypeThatIsTheProfile_SpecificContentTypeForTheResource()
        {
            VerifyAvailableContentTypes(HttpMethod.Put);
        }

        [Then(@"the POST operations should only provide a single option for the Parameter Content Type that is the profile-specific content type for the resource")]
        public void ThenThePOSTOperationsShouldOnlyProvideASingleOptionForTheParameterContentTypeThatIsTheProfile_SpecificContentTypeForTheResource()
        {
            VerifyAvailableContentTypes(HttpMethod.Post);
        }

        private static void VerifyAvailableContentTypes(HttpMethod method)
        {
            var browser = FeatureContext.Current.Get<BrowserSession>();
            string profileName = ScenarioContext.Current.Get<string>("profileName");

            var resourceItems = GetResourceItems(browser);

            ContentTypeUsage contentTypeUsage;

            if (method == HttpMethod.Get)
            {
                contentTypeUsage = ContentTypeUsage.Readable;
            }
            else if (method == HttpMethod.Put || method == HttpMethod.Post)
            {
                contentTypeUsage = ContentTypeUsage.Writable;
            }
            else
            {
                throw new NotSupportedException(
                    string.Format("Unable to verify content types for HTTP Method '{0}'.", method));
            }

            foreach (var resourceItem in resourceItems)
            {
                // Extract the resource collection name from the "id"
                string resourceCollectionName = resourceItem.Id.Split('_')[1];

                // Find the PUT operation LIs
                var putOperationItems = resourceItem.FindAllCss("li." + method.ToString().ToLower() + ".operation");

                foreach (var getOperationItem in putOperationItems)
                {
                    // Expand the operation
                    var putOperationLink = getOperationItem.FindCss("span.http_method a");
                    putOperationLink.Click();

                    // Find the only currently visible selection list
                    string classPrefix = contentTypeUsage == ContentTypeUsage.Writable
                        ? "parameter"
                        : "response";

                    var contentTypeList = getOperationItem.FindCss("div." + classPrefix + "-content-type select");

                    IEnumerable<SnapshotElementScope> options = contentTypeList.FindAllXPath("option");

                    string expectedContentType = ProfilesContentTypeHelper.CreateContentType(
                        resourceCollectionName,
                        profileName,
                        contentTypeUsage);

                    Assert.That(options.First().Text, Is.EqualTo(expectedContentType));
                }
            }
        }

        [When(@"I GetAll ""(.*)""")]
        public void WhenIGetAll(string resourceCollectionName)
        {
            ScenarioContext.Current.Set(resourceCollectionName, "Operation.ResourceCollectionName");

            var browser = FeatureContext.Current.Get<BrowserSession>();

            var resourceItem = browser.FindCss("li#resource_" + resourceCollectionName);
            
            // Expand the resource
            var resourceLink = resourceItem.FindCss("div.heading h2 a", new Options{Timeout = TimeSpan.FromSeconds(5)});
            resourceLink.Click();

            // Find the first GET operation LI
            var operationItem = resourceItem.FindCss("li.get.operation", new Options { Timeout = TimeSpan.FromSeconds(60), Match = Match.First});
            ScenarioContext.Current.Set(HttpMethod.Get, "Operation.Method");

            // Expand the operation
            var operationLink = operationItem.FindCss("span.http_method a", new Options { Match = Match.First, Timeout = TimeSpan.FromSeconds(5)});
            operationLink.Click();

            // Execute the get all request
            operationItem.ClickButton("Try it out!");

            var responseCodeElement = operationItem.FindCss("div.response_code", new Options{Timeout = TimeSpan.FromSeconds(3)});
            var responseHeadersElement = operationItem.FindCss("div.response_headers");

            ScenarioContext.Current.Set(responseCodeElement.Text, "Operation.ResponseCode");
            ScenarioContext.Current.Set(responseHeadersElement.Text, "Operation.ResponseHeaders");
        }

        [When(@"I (PUT|POST) a (.*)")]
        public void WhenIPost(string method, string resourceItemName)
        {
            HttpMethod httpMethod = method == "PUT" ? HttpMethod.Put : HttpMethod.Post;

            //ScenarioContext.Current.Set(resourceCollectionName, "Operation.ResourceCollectionName");
            var browser = FeatureContext.Current.Get<BrowserSession>();

            string resourceCollectionName = CompositeTermInflector.MakePlural(resourceItemName);

            bool resourceAlreadyExpanded =
                browser.FindCss(
                    "li#resource_" + resourceCollectionName + ".resource.active")
                    .Exists(new Options {Timeout = TimeSpan.FromMilliseconds(100)});

            var resourceItem = browser.FindCss("li#resource_" + resourceCollectionName);

            // Expand the resource
            if (!resourceAlreadyExpanded)
            {
                var resourceLink = resourceItem.FindCss("div.heading h2 a", 
                    new Options { Timeout = TimeSpan.FromSeconds(5) });
                resourceLink.Click();
            }

            // Find the first operation LI
            var operationItem = resourceItem.FindCss("li." + method.ToLower()  + ".operation",
                new Options { Timeout = TimeSpan.FromSeconds(60), Match = Match.First });
            
            ScenarioContext.Current.Set(httpMethod, "Operation.Method");

            // Expand the operation
            var operationLink = operationItem.FindCss("span.http_method a", 
                new Options { Match = Match.First, Timeout = TimeSpan.FromSeconds(5) });
            
            operationLink.Click();

            if (httpMethod == HttpMethod.Put)
            {
                // Set the id
                var idEntry = operationItem.FindCss("input.parameter[name='id']", 
                    new Options { Match = Match.First, Timeout = TimeSpan.FromSeconds(5) });

                idEntry.SendKeys(Guid.NewGuid().ToString("n"));
            }

            // Enter the JSON payload into the TEXTAREA control
            operationItem.FindCss("textarea").SendKeys(@"
{
    ""schoolId"": 9999,
    ""shortNameOfInstitution"": ""A-short-name"",
    ""webSite"": ""http://example.com"",
    ""operationalStatusType"": ""Mostly"",
    ""charterStatusType"": ""Something"",
    ""administrativeFundingControlDescriptor"": ""Controlling""
}",
                new Options {Timeout = TimeSpan.FromSeconds(5)});

            // Execute the get all request
            var button = operationItem.FindCss("input.submit", new Options { Timeout = TimeSpan.FromSeconds(5) });
            button.Click();

            var responseCodeElement = operationItem.FindCss("div.response_code", new Options { Timeout = TimeSpan.FromSeconds(3) });
            var responseHeadersElement = operationItem.FindCss("div.response_headers");

            ScenarioContext.Current.Set(responseCodeElement.Text, "Operation.ResponseCode");
            ScenarioContext.Current.Set(responseHeadersElement.Text, "Operation.ResponseHeaders");
        }

        [Then(@"the response code should indicate success")]
        public void ThenTheResponseCodeShouldIndicateSuccess()
        {
            string responseCode = ScenarioContext.Current.Get<string>("Operation.ResponseCode");

            Assert.That(responseCode, Is.EqualTo("200").Or.EqualTo("201").Or.EqualTo("204"));
        }

        [Then(@"the response headers should contain the profile-specific content type for the resource")]
        public void ThenTheResponseHeadersShouldContainTheProfile_SpecificContentTypeForTheResource()
        {
            string profileName = ScenarioContext.Current.Get<string>("profileName");
            string resourceCollectionName = ScenarioContext.Current.Get<string>("Operation.ResourceCollectionName");
            string responseHeaders = ScenarioContext.Current.Get<string>("Operation.ResponseHeaders");
            HttpMethod method = ScenarioContext.Current.Get<HttpMethod>("Operation.Method");

            string expectedContentType = ProfilesContentTypeHelper.CreateContentType(
                resourceCollectionName, profileName, method == HttpMethod.Get ? ContentTypeUsage.Readable : ContentTypeUsage.Writable);

            Assert.That(responseHeaders, Is.StringContaining(expectedContentType));
        }
    }
}
