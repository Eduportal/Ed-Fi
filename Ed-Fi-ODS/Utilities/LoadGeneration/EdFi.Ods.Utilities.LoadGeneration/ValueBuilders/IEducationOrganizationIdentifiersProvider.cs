// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using EdFi.Ods.Utilities.LoadGeneration.Security;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration.ValueBuilders
{
    public interface IEducationOrganizationIdentifiersProvider
    {
        IList<EducationOrganizationIdentifiers> GetEducationOrganizationIdentifiers();
        IList<EducationOrganizationIdentifiers> GetEducationOrganizationIdentifiers(string educationOrganizationType);
    }

    public class EducationOrganizationIdentifiers
    {
        public int EducationOrganizationId { get; set; }
        public string EducationOrganizationType { get; set; }

        public int? LocalEducationAgencyId { get; set; }
        public int? SchoolId { get; set; }
    }

    // TODO: Needs unit tests
    public class EducationOrganizationIdentifiersProvider : IEducationOrganizationIdentifiersProvider
    {
        private ILog _logger = LogManager.GetLogger(typeof(EducationOrganizationIdentifiersProvider));

        private readonly IApiSdkFacade _apiSdkFacade;
        private readonly IApiSdkReflectionProvider _apiSdkReflectionProvider;
        private readonly IApiSecurityContextProvider _apiSecurityContextProvider;

        public EducationOrganizationIdentifiersProvider(IApiSdkFacade apiSdkFacade, 
            IApiSdkReflectionProvider apiSdkReflectionProvider,
            IApiSecurityContextProvider _apiSecurityContextProvider)
        {
            _apiSdkFacade = apiSdkFacade;
            _apiSdkReflectionProvider = apiSdkReflectionProvider;
            this._apiSecurityContextProvider = _apiSecurityContextProvider;

            _educationOrganizationIdentifiers = GetLazyEducationOrganizationIdentifiers();
        }

        private readonly Lazy<List<EducationOrganizationIdentifiers>> _educationOrganizationIdentifiers;

        private Lazy<List<EducationOrganizationIdentifiers>> GetLazyEducationOrganizationIdentifiers()
        {
            return new Lazy<List<EducationOrganizationIdentifiers>>(() =>
            {
                Type schoolModelType;

                if (!_apiSdkReflectionProvider.TryGetModelType("School", out schoolModelType))
                    throw new Exception("Unable to locate School resource model type.");
                
                _logger.Info("Loading schools...");

                var schools = _apiSdkFacade.GetAll(schoolModelType);

                var schoolIdentifiers =
                    (from dynamic school in schools
                    select new EducationOrganizationIdentifiers
                    {
                        EducationOrganizationType = "School",
                        EducationOrganizationId = school.schoolId, 
                        LocalEducationAgencyId = school.localEducationAgencyReference.localEducationAgencyId, 
                        SchoolId = school.schoolId,
                    })
                    .ToList();

                var distinctLocalEducationAgencyIdentifiers =
                    (from r in schoolIdentifiers 
                    select r.LocalEducationAgencyId)
                    .Distinct()
                    .Select(leaId => new EducationOrganizationIdentifiers
                    {
                        EducationOrganizationType = "LocalEducationAgency",
                        EducationOrganizationId = leaId.Value, 
                        LocalEducationAgencyId = leaId,
                    });

                var apiSecurityContext = _apiSecurityContextProvider.GetSecurityContext();

                var accessibleLocalEducationAgencyIds = apiSecurityContext.LocalEducationAgencyIds;

                return 
                    (from ids in schoolIdentifiers.Concat(distinctLocalEducationAgencyIdentifiers)
                    where accessibleLocalEducationAgencyIds.Contains(ids.LocalEducationAgencyId.Value)
                    select ids)
                    .ToList();
            });
        }

        public IList<EducationOrganizationIdentifiers> GetEducationOrganizationIdentifiers()
        {
            return _educationOrganizationIdentifiers.Value;
        }

        private ConcurrentDictionary<string, IList<EducationOrganizationIdentifiers>> identiferListByOrgType = new ConcurrentDictionary<string, IList<EducationOrganizationIdentifiers>>(StringComparer.InvariantCultureIgnoreCase); 

        public IList<EducationOrganizationIdentifiers> GetEducationOrganizationIdentifiers(string educationOrganizationType)
        {
            // Get or create a list containing the specified ed org type
            return identiferListByOrgType.GetOrAdd(educationOrganizationType, (orgType) =>
                (from x in _educationOrganizationIdentifiers.Value
                 where x.EducationOrganizationType.Equals(orgType, StringComparison.InvariantCultureIgnoreCase)
                select x)
                .ToList()
            );
        }
    }
}