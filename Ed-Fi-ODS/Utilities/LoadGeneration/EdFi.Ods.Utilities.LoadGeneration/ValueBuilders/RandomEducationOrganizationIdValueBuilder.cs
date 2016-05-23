// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Utilities.LoadGeneration.Infrastructure;

namespace EdFi.Ods.Utilities.LoadGeneration.ValueBuilders
{
    public class RandomEducationOrganizationIdValueBuilder : RandomEducationOrganizationIdentifierValueBuilderBase
    {
        public RandomEducationOrganizationIdValueBuilder(IEducationOrganizationIdentifiersProvider educationOrganizationIdentifiersProvider,
            IRandom random)
        {
            _educationOrganizationIdentifiersProvider = educationOrganizationIdentifiersProvider;
            _random = random;
        }

        protected override IEnumerable<int> GetIdentifiers()
        {
            return _educationOrganizationIdentifiersProvider
                .GetEducationOrganizationIdentifiers()
                .Where(x => x.EducationOrganizationType == "School") // TODO: This introduced to try and avoid complexity of EdOrg authorization
                .Select(x => x.EducationOrganizationId);
        }

        protected override string HandledPropertyNameSuffix
        {
            get { return "EducationOrganizationId"; }
        }
    }
}