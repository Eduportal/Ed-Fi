// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

namespace EdFi.Ods.Tests.EdFi.Ods.Common.Security
{
    using System.Collections.Generic;
    using System.Linq;

    using global::EdFi.Common.Context;
    using global::EdFi.Ods.Common.Security;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Should;

    public class ApiKeyContextProviderTests
    {
        public class When_reading_EducationOrganization_type_and_identifiers_from_context_where_no_data_has_been_set : TestFixtureBase
        {
            private IEnumerable<int> actualEducationOrganizationIds;

            protected override void ExecuteBehavior()
            {
                var contextStorage = new HashtableContextStorage();
                var provider = new ApiKeyContextProvider(contextStorage);

                this.actualEducationOrganizationIds = provider.GetApiKeyContext().EducationOrganizationIds;
            }

            [Test]
            public void Should_return_an_empty_collection_of_Education_organization_identifiers()
            {
                actualEducationOrganizationIds.ShouldNotBeNull();
                actualEducationOrganizationIds.Count().ShouldEqual(0);
            }
        }
    }
}