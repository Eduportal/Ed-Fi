using System;
using System.IO;
using System.Reflection;
using System.Web.Http;
using Castle.MicroKernel.Registration;
using EdFi.Ods.Api.Startup;
using EdFi.Ods.WebApi.Startup;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup("TestProfilesSandbox", typeof(TestProfilesSandboxStartup))]

namespace EdFi.Ods.WebApi.Startup
{
    public class TestProfilesSandboxStartup : SandboxStartup
    {
        public override void Configuration(IAppBuilder appBuilder)
        {
            base.Configuration(appBuilder);

            const string testProfilesAssemblyName = "EdFi.Ods.Api.Models.TestProfiles";

            try
            {
                // Try to load the Test Profiles assembly from file 
                // (the assembly is obtained this way rather than using the marker interface 
                // so that the reference isn't required in the out-of-the-box Ed-Fi project)
                var testProfilesAssembly = Assembly.Load(testProfilesAssemblyName);

                // Register additional controllers for the test profiles
                Container.Register(
                    Classes.FromAssembly(testProfilesAssembly)
                           .BasedOn<ApiController>()
                           .LifestyleTransient());
            }
            catch (FileNotFoundException ex)
            {
                throw new Exception("Unable to find assembly '" + testProfilesAssemblyName + "'.  Did you add a reference or explicit dependency to it in your 'EdFi.Ods.WebApi' project?", ex);
            }
        }
    }
}