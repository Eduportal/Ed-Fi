﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.34209
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace EdFi.Ods.WebService.Tests.Profiles
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Profile definitions can be readable or writable only")]
    [NUnit.Framework.CategoryAttribute("API")]
    public partial class ProfileDefinitionsCanBeReadableOrWritableOnlyFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Profile definitions can be readable or writable only.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Profile definitions can be readable or writable only", "", ProgrammingLanguage.CSharp, new string[] {
                        "API"});
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("A GET request is made with a read only profile")]
        public virtual void AGETRequestIsMadeWithAReadOnlyProfile()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A GET request is made with a read only profile", ((string[])(null)));
#line 5
this.ScenarioSetup(scenarioInfo);
#line 6
    testRunner.Given("the caller is using the \"Test-Profile-Resource-ReadOnly\" profile", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 7
    testRunner.When("a GET (by id) request is submitted to schools with an accept header content type " +
                    "of the appropriate value for the profile in use", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 8
    testRunner.Then("the response should indicate success", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("A PUT request is made with a read only profile")]
        public virtual void APUTRequestIsMadeWithAReadOnlyProfile()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A PUT request is made with a read only profile", ((string[])(null)));
#line 10
this.ScenarioSetup(scenarioInfo);
#line 11
    testRunner.Given("the caller is using the \"Test-Profile-Resource-ReadOnly\" profile", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 12
    testRunner.When("a PUT request with a completely updated resource is submitted using raw JSON to s" +
                    "chools with a request body content type of the appropriate value for the profile" +
                    " in use", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 13
    testRunner.Then("the response should contain a 405 Method Not Allowed failure indicating that \"The" +
                    " allowed methods for this resource with the \'{profile}\' profile are GET, DELETE " +
                    "and OPTIONS.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("A POST request is made with a read only profile")]
        public virtual void APOSTRequestIsMadeWithAReadOnlyProfile()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A POST request is made with a read only profile", ((string[])(null)));
#line 15
this.ScenarioSetup(scenarioInfo);
#line 16
    testRunner.Given("the caller is using the \"Test-Profile-Resource-ReadOnly\" profile", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 17
    testRunner.When("a POST request with a resource is submitted to schools with a request body conten" +
                    "t type of the appropriate value for the profile in use", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 18
    testRunner.Then("the response should contain a 405 Method Not Allowed failure indicating that \"The" +
                    " allowed methods for this resource with the \'{profile}\' profile are GET, DELETE " +
                    "and OPTIONS.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("A GET request is made with a write only profile")]
        public virtual void AGETRequestIsMadeWithAWriteOnlyProfile()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A GET request is made with a write only profile", ((string[])(null)));
#line 21
this.ScenarioSetup(scenarioInfo);
#line 22
    testRunner.Given("the caller is using the \"Test-Profile-Resource-WriteOnly\" profile", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 23
    testRunner.When("a GET (by id) request is submitted to schools with an accept header content type " +
                    "of the appropriate value for the profile in use", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 24
    testRunner.Then("the response should contain a 405 Method Not Allowed failure indicating that \"The" +
                    " allowed methods for this resource with the \'{profile}\' profile are PUT, POST, D" +
                    "ELETE and OPTIONS.\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("A PUT request is made with a write only profile")]
        public virtual void APUTRequestIsMadeWithAWriteOnlyProfile()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A PUT request is made with a write only profile", ((string[])(null)));
#line 26
this.ScenarioSetup(scenarioInfo);
#line 27
    testRunner.Given("the caller is using the \"Test-Profile-Resource-WriteOnly\" profile", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 28
    testRunner.When("a PUT request with a completely updated resource is submitted using raw JSON to s" +
                    "chools with a request body content type of the appropriate value for the profile" +
                    " in use", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 29
    testRunner.Then("the response should indicate success", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("A POST request is made with a write only profile")]
        public virtual void APOSTRequestIsMadeWithAWriteOnlyProfile()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("A POST request is made with a write only profile", ((string[])(null)));
#line 31
this.ScenarioSetup(scenarioInfo);
#line 32
    testRunner.Given("the caller is using the \"Test-Profile-Resource-WriteOnly\" profile", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 33
    testRunner.When("a POST request with a resource is submitted to schools with a request body conten" +
                    "t type of the appropriate value for the profile in use", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 34
    testRunner.Then("the response should indicate success", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
