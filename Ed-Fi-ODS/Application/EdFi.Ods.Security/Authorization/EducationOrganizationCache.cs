// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using EdFi.Common.Caching;
using EdFi.Common.Configuration;
using EdFi.Common.Database;
using EdFi.Common.Security;

namespace EdFi.Ods.Security.Authorization
{
    public class EducationOrganizationCache : IEducationOrganizationCache
    {
        private const string CacheKeyFormat = "EducationOrganizationCache.List.{0}";
        public const string CacheExpirationMinutesAppSettingsKey = "EducationOrganizationCache.ExpirationMinutes";

        private readonly ICacheProvider cacheProvider;
        private readonly IDatabaseConnectionStringProvider odsDatabaseConnectionStringProvider;
        private readonly IConfigValueProvider configValueProvider;
        private readonly IEducationOrganizationCacheDataProvider educationOrganizationCacheDataProvider;

        private static readonly EducationOrganizationIdentifiersComparer Comparer = new EducationOrganizationIdentifiersComparer();

        public EducationOrganizationCache(ICacheProvider cacheProvider, IDatabaseConnectionStringProvider odsDatabaseConnectionStringProvider, IConfigValueProvider configValueProvider, IEducationOrganizationCacheDataProvider educationOrganizationCacheDataProvider)
        {
            this.cacheProvider = cacheProvider;
            this.odsDatabaseConnectionStringProvider = odsDatabaseConnectionStringProvider;
            this.configValueProvider = configValueProvider;
            this.educationOrganizationCacheDataProvider = educationOrganizationCacheDataProvider;
        }

        private string GetCurrentCacheKey()
        {
            return string.Format(CacheKeyFormat, odsDatabaseConnectionStringProvider.GetConnectionString().GetHashCode());
        }

        /// <summary>
        /// Finds the <see cref="EducationOrganizationIdentifiers"/> for the specified <paramref name="educationOrganizationId"/>, or <b>null</b>.
        /// </summary>
        /// <param name="educationOrganizationId">The generic Education Organization identifier for which to search.</param>
        /// <returns>The matching <see cref="EducationOrganizationIdentifiers"/>; otherwise <b>null</b>.</returns>
        public EducationOrganizationIdentifiers GetEducationOrganizationIdentifiers(int educationOrganizationId)
        {
            List<EducationOrganizationIdentifiers> list;

            object value;

            string currentCacheKey = GetCurrentCacheKey();

            if (!cacheProvider.TryGetCachedObject(currentCacheKey, out value))
            {
                list = educationOrganizationCacheDataProvider.GetEducationOrganizationIdentifiers();
                string expirationMinutesText = configValueProvider.GetValue(CacheExpirationMinutesAppSettingsKey);
                int expirationMinutes = Convert.ToInt32(expirationMinutesText ?? "30");
                cacheProvider.Insert(currentCacheKey, list, DateTime.Now.AddMinutes(expirationMinutes), TimeSpan.Zero);
            }
            else
            {
                list = (List<EducationOrganizationIdentifiers>) value;
            }

            int foundIndex = list.BinarySearch(EducationOrganizationIdentifiers.CreateLookupInstance(educationOrganizationId), Comparer);

            if (foundIndex < 0)
                return null;

            return list[foundIndex];
        }

        private class EducationOrganizationIdentifiersComparer : IComparer<EducationOrganizationIdentifiers>
        {
            public int Compare(EducationOrganizationIdentifiers x, EducationOrganizationIdentifiers y)
            {
                return x.EducationOrganizationId.CompareTo(y.EducationOrganizationId);
            }
        }
    }
}