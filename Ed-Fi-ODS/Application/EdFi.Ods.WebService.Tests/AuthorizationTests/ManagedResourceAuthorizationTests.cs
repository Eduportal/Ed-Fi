using EdFi.Ods.Api.Models.Resources.AcademicSubjectDescriptor;
using EdFi.Ods.Security.Metadata.Contexts;
using EdFi.Ods.Security.Metadata.Models;
using EdFi.Ods.WebService.Tests.Extensions;
using EdFi.Ods.WebService.Tests.Owin;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace EdFi.Ods.WebService.Tests.AuthorizationTests
{
    [TestFixture]
    public class NamespaceBasedAuthorizationTests : OwinTestBase
    {
        private static readonly string _databaseName = string.Format("EdFi_Tests_NamespaceBasedAuthorizationTests_{0}", Guid.NewGuid().ToString("N"));
        private readonly List<int> LocalEducationAgencyIds = new List<int> { 255901 };

        protected override string DatabaseName { get { return _databaseName; } }
        protected override string BaseDatabase { get { return "EdFi_Ods_Populated_Template"; } }

        private const string TestNamespace = "http://www.TEST.org/";

        private static OwinSecurityRepository CreateSecurityRepository()
        {
            var securityRepo = new OwinSecurityRepository();

            var descriptor = securityRepo.GetResourceClaims().First(rc => rc.ResourceName.Equals("academicSubjectDescriptor"));
            var authorizationStrategy = securityRepo.GetAuthorizationStrategies().First(auth => auth.AuthorizationStrategyName.Equals("NamespaceBased"));
            var claimset = securityRepo.GetClaimSets().First(cs => cs.ClaimSetName.Equals("SIS Vendor"));

            var resourceClaimAuthorizationStrategies = securityRepo.GetResourceClaimAuthorizationStrategies();
            var claimSetResourceClaims = securityRepo.GetClaimSetResourceClaims();

            foreach (var action in securityRepo.GetActions())
            {
                resourceClaimAuthorizationStrategies.Add(new ResourceClaimAuthorizationStrategy
                {
                    Action = action,
                    AuthorizationStrategy = authorizationStrategy,
                    ResourceClaim = descriptor
                });

                claimSetResourceClaims.Add(new ClaimSetResourceClaim
                {
                    Action = action,
                    ClaimSet = claimset,
                    ResourceClaim = descriptor
                });
            }
            
            securityRepo.ReIntitalize(securityRepo.GetApplication(), securityRepo.GetActions(), securityRepo.GetClaimSets(), securityRepo.GetResourceClaims(), securityRepo.GetAuthorizationStrategies(), claimSetResourceClaims, resourceClaimAuthorizationStrategies);

            return securityRepo;
        }
        
        [Test]
        public void Create_Fail()
        {
            var academicSubjectDescriptor = new AcademicSubjectDescriptor
            {
                AcademicSubjectType = "Foreign Language and Literature",
                CodeValue = "9999",
                Description = "German",
                EffectiveBeginDate = DateTime.UtcNow,
                EffectiveEndDate = null,
                Namespace = "http://www.FAIL.org/Descriptor/AcademicSubjectDescriptor.xml",
                ShortDescription = "German",
            };

            var message = JsonConvert.SerializeObject(academicSubjectDescriptor);

            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace, null, null, CreateSecurityRepository))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var result = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "AcademicSubjectDescriptors"), new StringContent(message, Encoding.UTF8, "application/json")).Result;
                        result.IsSuccessStatusCode.ShouldBeFalse();
                    }
                }
            }
        }

        [Test]
        public void Create_Success()
        {
            var academicSubjectDescriptor = new AcademicSubjectDescriptor
            {
                AcademicSubjectType = "Foreign Language and Literature",
                CodeValue = "9999",
                Description = "German",
                EffectiveBeginDate = DateTime.UtcNow,
                EffectiveEndDate = null,
                Namespace = "http://www.TEST.org/Descriptor/AcademicSubjectDescriptor.xml",
                ShortDescription = "German",
            };

            var message = JsonConvert.SerializeObject(academicSubjectDescriptor);

            using (var startup = new OwinStartup(DatabaseName, LocalEducationAgencyIds, TestNamespace, null, null, CreateSecurityRepository))
            {
                using (var server = TestServer.Create(startup.Configuration))
                {
                    using (var client = new HttpClient(server.Handler))
                    {
                        client.Timeout = new TimeSpan(0, 0, 15, 0);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Guid.NewGuid().ToString());

                        var result = client.PostAsync(OwinUriHelper.BuildApiUri("2014", "AcademicSubjectDescriptors"), new StringContent(message, Encoding.UTF8, "application/json")).Result;
                        result.EnsureSuccessStatusCode();
                    }
                }
            }
        }
    }
}
