using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.SecurityConfiguration.Services.Model;
using VendorEntity = EdFi.Ods.Admin.Models.Vendor;
using ApplicationEntity = EdFi.Ods.Admin.Models.Application;
using ProfileEntity = EdFi.Ods.Admin.Models.Profile;
using ApplicationModel = EdFi.Ods.SecurityConfiguration.Services.Model.Application;
using ProfileModel = EdFi.Ods.SecurityConfiguration.Services.Model.Profile;

namespace EdFi.Ods.SecurityConfiguration.Services.Implementation
{
    public class ApplicationService : VendorAndApplicationBase, IApplicationService
    {
        private readonly IAdminContextFactory _context;
        private readonly IEducationOrganizationProvider _educationOrganizationProvider;
        private readonly IKeyRetrievalChallengeService _keyRetrievalChallengeService;
        private readonly IEmailService _emailService;
        private readonly ICredentialService _credentialService;

        public ApplicationService(
            IAdminContextFactory context,
            IEducationOrganizationProvider educationOrganizationProvider,
            IKeyRetrievalChallengeService keyRetrievalChallengeService, IEmailService emailService,
            ICredentialService credentialService)
        {
            _context = context;
            _educationOrganizationProvider = educationOrganizationProvider;
            _keyRetrievalChallengeService = keyRetrievalChallengeService;
            _emailService = emailService;
            _credentialService = credentialService;
        }

        private VendorEntity GetVendor(IUsersContext context, int vendorId)
        {
            var vendor = context.Vendors.SingleOrDefault(v => v.VendorId == vendorId);
            if (vendor == null)
                throw new ArgumentException(string.Format("cannot find vendor id '{0}'", vendorId));

            return vendor;
        }

        private static User GetVendorUser(VendorEntity vendor)
        {
            var vendorUser = vendor.Users.FirstOrDefault();
            if (vendorUser == null)
                throw new DataException(string.Format("vendor '[id:{0}, name: {1}] does not have a Contact", vendor.VendorId, vendor.VendorName));
            return vendorUser;
        }

        private IEnumerable<ApplicationModel> GetAllVendorApplications(IUsersContext context, int vendorId, bool revealSecret = false)
        {
            var vendor = GetVendor(context, vendorId);
            var vendorUser = GetVendorUser(vendor);

            var apps = (
                from appEntity in context.Applications
                let apiClient =
                    appEntity.ApiClients.FirstOrDefault(ac => ac.User.UserId == vendorUser.UserId && !ac.UseSandbox)
                where appEntity.Vendor.VendorId == vendorId
                      && apiClient != null
                      && !apiClient.UseSandbox
                select new
                {
                    appEntity,
                    apiClient,
                    expired = apiClient.ChallengeExpiry != null && apiClient.ChallengeExpiry < DateTime.Now
                })
                .ToList()
                .Select(a => new ApplicationModel
                {
                    ApplicationId = a.appEntity.ApplicationId,
                    ApplicationName = a.appEntity.ApplicationName,
                    ApiKey = a.apiClient.Key,
                    ApiSecret = revealSecret ? a.apiClient.Secret : "********",
                    ActivationCode = a.expired ? null : a.apiClient.ActivationCode,
                    AssociatedProfiles = a.appEntity.Profiles.Select(p => new ProfileModel
                    {
                        ProfileId = p.ProfileId,
                        ProfileName = p.ProfileName,
                    }),
                    ClaimSetName = a.appEntity.ClaimSetName,
                    EducationOrganizations =
                        a.appEntity.ApplicationEducationOrganizations.Select(lea =>
                        {
                            var edOrg = _educationOrganizationProvider
                                .GetAll()
                                .SingleOrDefault(e => e.EducationOrganizationId == lea.EducationOrganizationId);

                            return new EducationOrganization
                            {
                                EducationOrganizationId = edOrg == null ? 0 : edOrg.EducationOrganizationId,
                                EducationOrganizationName = edOrg == null ? null : edOrg.EducationOrganizationName
                            };
                        }),
                    KeyStatus = a.expired ? KeyStatus.Expired : a.apiClient.KeyStatus,
                });

            return apps;
        }

        public IEnumerable<ApplicationModel> GetVendorApplications(int vendorId)
        {
            using (var context = _context.Create())
            {
                return GetAllVendorApplications(context, vendorId).ToList();
            }
        }

        public ApplicationModel GetById(int vendorId, int applicationId)
        {
            using (var context = _context.Create())
            {
                var collection = GetAllVendorApplications(context, vendorId);
                if (collection == null) return null;
                return collection
                    .SingleOrDefault(a => a.ApplicationId == applicationId);
            }
        }

        public int AddApplication(int vendorId, ApplicationModel newApplication)
        {
            using (var context = _context.Create())
            {
                var vendor = GetVendor(context, vendorId);
                var mainUser = GetVendorUser(vendor);

                // sanitize data:
                newApplication.ApplicationName = newApplication.ApplicationName.Trim();


                // data validation:
                if (context.Applications.Any(
                        a => a.ApplicationName == newApplication.ApplicationName && a.Vendor.VendorId == vendorId))
                {
                    throw new DuplicateNameException(string.Format("Application '{0}' already exists", newApplication.ApplicationName));
                }

                // prepare entities:
                var appEntity = new ApplicationEntity
                {
                    ApplicationName = newApplication.ApplicationName,
                    Vendor = vendor,
                    ClaimSetName = newApplication.ClaimSetName
                };

                foreach (var edOrg in newApplication.EducationOrganizations.ToList())
                {
                    appEntity.ApplicationEducationOrganizations.Add(new ApplicationEducationOrganization
                    {
                        EducationOrganizationId = edOrg.EducationOrganizationId
                    });
                }

                var newCredentials = _credentialService.GetNewCredentials();

                var apiClientEntity = new ApiClient
                {
                    Name = "Auto-Generated",
                    Key = newCredentials.Key, // can't leave it blank
                    Secret = newCredentials.Secret, // can't leave it blank
                    IsApproved = true,
                    UseSandbox = false,
                    SandboxType = 0,
                    User = mainUser,
                    KeyStatus = KeyStatus.Initialized
                };

                foreach (var edOrg in appEntity.ApplicationEducationOrganizations.ToList())
                {
                    apiClientEntity.ApplicationEducationOrganizations.Add(edOrg);
                }

                foreach (var profile in newApplication.AssociatedProfiles.ToList())
                {
                    var profileEntity = context.Profiles.SingleOrDefault(p => p.ProfileId == profile.ProfileId);
                    appEntity.Profiles.Add(profileEntity);
                }

                appEntity.ApiClients.Add(apiClientEntity);

                context.Applications.Add(appEntity);

                context.SaveChanges();

                return appEntity.ApplicationId;
            }
        }

        public void DeleteApplication(int vendorId, int applicationId)
        {
            using (var context = _context.Create())
            {
                var app = context.Applications.SingleOrDefault(
                    a => a.Vendor.VendorId == vendorId && a.ApplicationId == applicationId);

                if (app == null)
                    throw new ArgumentException(string.Format("cannot find application id '{0}' for the vendor '{1}",
                        applicationId, vendorId));

                RemoveApplicationHierarchy(context, app);
                context.SaveChanges();
            }
        }

        public void UpdateApplication(int vendorId, ApplicationModel updatingApplication)
        {
            using (var context = _context.Create())
            {
                var vendor = GetVendor(context, vendorId);
                var mainUser = GetVendorUser(vendor);

                var appEntity =
                    context.Applications.SingleOrDefault(a => a.ApplicationId == updatingApplication.ApplicationId && a.Vendor.VendorId == vendorId);

                if(appEntity == null)
                    throw new ArgumentException(string.Format("cannot find applicationId: '{0}'", updatingApplication.ApplicationId));

                // sanitize data:
                updatingApplication.ApplicationName = updatingApplication.ApplicationName.Trim();

                // data validation:
                if (context.Applications.Any(a =>
                    a.ApplicationName == updatingApplication.ApplicationName &&
                    a.Vendor.VendorId == vendorId &&
                    a.ApplicationId != updatingApplication.ApplicationId))
                {
                    throw new DuplicateNameException(string.Format("Application '{0}' already exists",
                        updatingApplication.ApplicationName));
                }

                // prepare entities:
                appEntity.ApplicationName = updatingApplication.ApplicationName;
                appEntity.Vendor = vendor;
                appEntity.ClaimSetName = updatingApplication.ClaimSetName;

                foreach (var lea in appEntity.ApplicationEducationOrganizations.ToList())
                {
                    appEntity.ApplicationEducationOrganizations.Remove(lea);
                }

                foreach (var edOrg in updatingApplication.EducationOrganizations.ToList())
                {
                    appEntity.ApplicationEducationOrganizations.Add(new ApplicationEducationOrganization
                    {
                        EducationOrganizationId = edOrg.EducationOrganizationId
                    });
                }

                var newCredentials = _credentialService.GetNewCredentials();

                var apiClientEntity = appEntity.ApiClients.FirstOrDefault(ac => !ac.UseSandbox) ??
                                      new ApiClient
                                      {
                                          Name = "Auto-Generated",
                                          Key = newCredentials.Key, // can't leave it blank
                                          Secret = newCredentials.Secret, // can't leave it blank
                                          UseSandbox = false
                                      };

                apiClientEntity.IsApproved = true;
                apiClientEntity.SandboxType = 0;
                apiClientEntity.User = mainUser;

                foreach (var lea in apiClientEntity.ApplicationEducationOrganizations.ToList())
                {
                    apiClientEntity.ApplicationEducationOrganizations.Remove(lea);
                }
                
                foreach (var edOrg in appEntity.ApplicationEducationOrganizations.ToList())
                {
                    apiClientEntity.ApplicationEducationOrganizations.Add(edOrg);
                }

                appEntity.ApiClients.Add(apiClientEntity);
                
                // remove profiles that are not in the new list
                var allNewProfileIds = updatingApplication.AssociatedProfiles.Select(ap => ap.ProfileId);

                ProfileEntity toBeRemovedProfile;
                while ((
                    toBeRemovedProfile = appEntity.Profiles
                        .FirstOrDefault(p => !allNewProfileIds.Contains(p.ProfileId))
                    ) != null)
                {
                    appEntity.Profiles.Remove(toBeRemovedProfile);
                }

                // calculate deltas
                var alreadyAssociated = appEntity.Profiles.Select(p => p.ProfileId);
                var toBeAddProfiles = context.Profiles
                    .Where(p => allNewProfileIds.Contains(p.ProfileId)
                                && !alreadyAssociated.Contains(p.ProfileId)
                    );

                // add newly added profiles
                foreach (var profile in toBeAddProfiles.ToList())
                    appEntity.Profiles.Add(profile);

                context.SaveChanges();
            }
        }

        public KeyGenResult GenerateApplicationKey(int vendorId, int applicationId)
        {
            User vendorUser;
            using (var context = _context.Create())
            {
                var vendor = GetVendor(context, vendorId);
                vendorUser = GetVendorUser(vendor);
            }

            var application = GetById(vendorId, applicationId);
            if (application == null)
                throw new ArgumentException(string.Format("cannot find application id: '{0}' for the vendor: '{1}'",
                    vendorId, applicationId));

            var keyGenResult = _keyRetrievalChallengeService.GenerateKeySecretAndChallenge(applicationId);
            
            var emailParameters = new EmailParameters
            {
                RecipientEmailAddress = vendorUser.Email,
                ChallengeId = keyGenResult.ChallengeId,
                ApplicationName = application.ApplicationName,
                EducationOrganization = application.EducationOrganizations.Select(eo => eo.EducationOrganizationName)
            };
            _emailService.SendActivationRequestEmail(emailParameters);

            // since ChallengeId is emailed to the client, for the security reason
            // we can hide it from the user of this call.
            keyGenResult.ChallengeId = "********";

            keyGenResult.KeyStatus = KeyStatus.Sent;

            return keyGenResult;
        }
    }
}