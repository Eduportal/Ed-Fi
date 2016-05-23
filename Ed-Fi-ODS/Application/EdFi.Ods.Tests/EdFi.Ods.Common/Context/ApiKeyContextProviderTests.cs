using System.Collections.Generic;
using EdFi.Common.Context;
using EdFi.Ods.Common.Security;
using EdFi.Ods.Tests._Bases;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.Common.Context
{
    public class ApiKeyContextProviderTests
    {
        public class When_setting_and_getting_the_ApiKey_context_values_individually : TestFixtureBase
        {
            private const string suppliedApiKey = "SomeKey";
            private const string suppliedClaimsetName = "SomeClaimset";
            private int[] suppliedEducationOrganizationIds = {1,2};
            private const string suppliedNamespacePrefix = "SomeNamespacePrefix";
            private IList<string> suppliedProfiles = new List<string> { "Profile1", "Profile2" };
            private ApiKeyContext suppliedApiKeyContext = new ApiKeyContext(null, null, null, null, null);

            private string actualApiKey;
            private string actualClaimsetName;
            private IEnumerable<int> actualEducationOrganizationIds;
            private string actualNamespacePrefix;
            private ApiKeyContext actualApiKeyContext;
            private IEnumerable<string> actualProfiles;
            private HashtableContextStorage _contextStorage;

            protected override void Act()
            {
                _contextStorage = new HashtableContextStorage();
                
                // Set the values
                var settingProvider = new ApiKeyContextProvider(_contextStorage);
                settingProvider.SetApiKeyContext(new ApiKeyContext(suppliedApiKey, suppliedClaimsetName, suppliedEducationOrganizationIds, suppliedNamespacePrefix, suppliedProfiles));

                // Get the values
                var gettingProvider = new ApiKeyContextProvider(_contextStorage);
                actualApiKey = gettingProvider.GetApiKeyContext().ApiKey;
                actualClaimsetName = gettingProvider.GetApiKeyContext().ClaimSetName;
                actualEducationOrganizationIds = gettingProvider.GetApiKeyContext().EducationOrganizationIds;
                actualNamespacePrefix = gettingProvider.GetApiKeyContext().NamespacePrefix;
                actualProfiles = gettingProvider.GetApiKeyContext().Profiles;

                // Set the context as a DTO
                settingProvider.SetApiKeyContext(suppliedApiKeyContext);

                // Get the context as a DTO
                actualApiKeyContext = gettingProvider.GetApiKeyContext();
            }

            [Assert]
            public void Should_return_the_supplied_ApiKey()
            {
                actualApiKey.ShouldEqual(suppliedApiKey);
            }

            [Assert]
            public void Should_return_the_supplied_claimset_name()
            {
                actualClaimsetName.ShouldEqual(suppliedClaimsetName);
            }

            [Assert]
            public void Should_return_the_supplied_EducationOrganizationIds()
            {
                actualEducationOrganizationIds.ShouldEqual(suppliedEducationOrganizationIds);
            }

            [Assert]
            public void Should_return_the_supplied_namespace_prefix()
            {
                actualNamespacePrefix.ShouldEqual(suppliedNamespacePrefix);
            }

            [Assert]
            public virtual void Should_return_the_supplied_context_DTO()
            {
                actualApiKeyContext.ShouldBeSameAs(suppliedApiKeyContext);
            }

            [Assert]
            public virtual void Should_return_the_supplied_profiles()
            {
                actualProfiles.ShouldBeSameAs(suppliedProfiles);
            }

            [Assert]
            public void Should_make_use_of_the_supplied_context_storage()
            {
                _contextStorage.UnderlyingHashtable.Count.ShouldBeGreaterThan(0);
            }
        }
    }
}
