// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System.Runtime.Caching;
using EdFi.Common.Security;

namespace EdFi.Ods.Tests.EdFi.Ods.Security.Authorization
{
    using System.Collections.Generic;

    using global::EdFi.Common.Caching;
    using global::EdFi.Common.Configuration;
    using global::EdFi.Common.Database;
    using global::EdFi.Ods.Security.Authorization;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    public class EducationOrganizationCacheTests
    {
        /// <summary>
        /// Provides a simple implementation of a connection string provider that exposes a property 
        /// for the value to be returned.
        /// </summary>
        class FakeConnectionStringProvider : IDatabaseConnectionStringProvider
        {
            public string CurrentValue { get; set; }

            public string GetConnectionString()
            {
                return this.CurrentValue;
            }
        }

        /// <summary>
        /// Provides an cache data provider implementation that enables the definition of return values
        /// based on specific connection string values.
        /// </summary>
        class FakeEducationOrganizationCacheDataProvider : IEducationOrganizationCacheDataProvider
        {
            private readonly IDatabaseConnectionStringProvider connectionStringProvider;

            private Dictionary<string, List<EducationOrganizationIdentifiers>> resultsByConnectionString 
                = new Dictionary<string, List<EducationOrganizationIdentifiers>>();

            public FakeEducationOrganizationCacheDataProvider(IDatabaseConnectionStringProvider connectionStringProvider)
            {
                this.connectionStringProvider = connectionStringProvider;
            }

            public void AddResult(string connectionStringValue, List<EducationOrganizationIdentifiers> result)
            {
                this.resultsByConnectionString[connectionStringValue] = result;
            }

            public List<EducationOrganizationIdentifiers> GetEducationOrganizationIdentifiers()
            {
                return this.resultsByConnectionString[this.connectionStringProvider.GetConnectionString()];
            }
        }

        public class When_retrieving_education_organization_cache_data_using_different_ODS_connection_strings : TestFixtureBase
        {
            private EducationOrganizationIdentifiers actual88ResultForString1;
            private EducationOrganizationIdentifiers actual99ResultForString1;
            private EducationOrganizationIdentifiers actual88ResultForString2;
            private EducationOrganizationIdentifiers actual99ResultForString2;

            protected override void ExecuteBehavior()
            {
                // Provide external dependencies not needing specific behavior in this test
                var cacheProvider = new MemoryCacheProvider();
                cacheProvider.MemoryCache = new MemoryCache("IsolatedForUnitTest");

                var configValueProvider = this.mocks.Stub<IConfigValueProvider>();

                // Create Faked dependencies
                var connectionStringProvider = new FakeConnectionStringProvider();
                var educationOrganizationCacheDataProvider = new FakeEducationOrganizationCacheDataProvider(connectionStringProvider);

                var suppliedIdentifierSet1 = new List<EducationOrganizationIdentifiers>()
                {
                    new EducationOrganizationIdentifiers(9, "LocalEducationAgency", 1, null, 9, null),
                    new EducationOrganizationIdentifiers(99, "School", 1, null, 9, 99),
                    new EducationOrganizationIdentifiers(999, "School", 1, null, 9, 999),
                };
                
                var suppliedIdentifierSet2 = new List<EducationOrganizationIdentifiers>()
                {
                    new EducationOrganizationIdentifiers(8, "LocalEducationAgency", 1, null, 8, null),
                    new EducationOrganizationIdentifiers(88, "School", 1, null, 8, 88),
                    new EducationOrganizationIdentifiers(888, "School", 1, null, 8, 888),
                };

                // Set up the cache data provider to return different data based on different connection strings
                educationOrganizationCacheDataProvider.AddResult("String1", suppliedIdentifierSet1);
                educationOrganizationCacheDataProvider.AddResult("String2", suppliedIdentifierSet2);

                // Create the cache
                var edOrgCache = new EducationOrganizationCache(cacheProvider, connectionStringProvider, 
                    configValueProvider, educationOrganizationCacheDataProvider);

                // First retrieve values for the first connection string
                connectionStringProvider.CurrentValue = "String1";
                this.actual88ResultForString1 = edOrgCache.GetEducationOrganizationIdentifiers(88);
                this.actual99ResultForString1 = edOrgCache.GetEducationOrganizationIdentifiers(99);

                // Then retrieve values for the second connection string
                connectionStringProvider.CurrentValue = "String2";
                this.actual88ResultForString2 = edOrgCache.GetEducationOrganizationIdentifiers(88);
                this.actual99ResultForString2 = edOrgCache.GetEducationOrganizationIdentifiers(99);
            }

            [Test]
            public void Should_return_cached_data_retrieved_when_the_current_connection_matches_the_connection_where_the_requested_data_is_defined()
            {
                this.actual99ResultForString1.LocalEducationAgencyId.ShouldEqual(9);
                this.actual88ResultForString2.LocalEducationAgencyId.ShouldEqual(8);
            }

            [Test]
            public void Should_return_no_data_when_the_current_connection_does_not_match_the_connection_where_the_requested_data_is_defined()
            {
                this.actual88ResultForString1.ShouldBeNull();
                this.actual99ResultForString2.ShouldBeNull();
            }
        }
    }
}