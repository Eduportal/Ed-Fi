using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.SecurityConfiguration.Services.Model;
using VendorEntity = EdFi.Ods.Admin.Models.Vendor;
using VendorModel = EdFi.Ods.SecurityConfiguration.Services.Model.Vendor;

namespace EdFi.Ods.SecurityConfiguration.Services.Implementation
{
    public class VendorService : VendorAndApplicationBase, IVendorService
    {
        private readonly IAdminContextFactory _context;

        public VendorService(IAdminContextFactory context)
        {
            _context = context;
        }

        private static IEnumerable<VendorModel> GetAll(IUsersContext context)
        {
            return from vendor in context.Vendors
                let firstContact = vendor.Users.FirstOrDefault()
                select new VendorModel
                {
                    VendorId = vendor.VendorId,
                    VendorName = vendor.VendorName,
                    NamespacePrefix = vendor.NamespacePrefix,
                    MainContact = new VendorContact
                    {
                        ContactId = firstContact == null ? 0 : firstContact.UserId,
                        ContactName = firstContact == null ? "" : firstContact.FullName,
                        ContactEmailAddress = firstContact == null ? "" : firstContact.Email
                    },
                    ApplicationCount = vendor.Applications.Count(app => app.ApiClients.Any(api => !api.UseSandbox)),
                    SandboxesCount = vendor.Applications.Count(app => app.ApiClients.Any(api => api.UseSandbox)),
                };
        }

        public IEnumerable<VendorModel> GetAll()
        {
            using (var context = _context.Create())
            {
                var result = GetAll(context).ToList();
                
                return result;
            }
        }

        public VendorModel GetById(int vendorId)
        {
            using (var context = _context.Create())
            {
                return GetAll(context)
                    .SingleOrDefault(v => v.VendorId == vendorId);
            }
        }

        public int AddVendor(VendorModel newVendor)
        {
            using (var context = _context.Create())
            {
                // sanitize data:
                newVendor.VendorName = newVendor.VendorName.Trim();
                newVendor.NamespacePrefix = newVendor.NamespacePrefix == null ? null : newVendor.NamespacePrefix.Trim();
                newVendor.MainContact.ContactName = newVendor.MainContact.ContactName.Trim();
                newVendor.MainContact.ContactEmailAddress = newVendor.MainContact.ContactEmailAddress.Trim();

                // data validation:
                if (context.Vendors.Any(vendor => vendor.VendorName == newVendor.VendorName))
                {
                    throw new DuplicateNameException(string.Format("Vendor '{0}' already exists", newVendor.VendorName));
                }

                var vendorEntity = new VendorEntity
                {
                    VendorName = newVendor.VendorName,
                    NamespacePrefix = newVendor.NamespacePrefix,
                };
                vendorEntity.Users.Add(new User
                {
                    FullName = newVendor.MainContact.ContactName,
                    Email = newVendor.MainContact.ContactEmailAddress
                });

                context.Vendors.Add(vendorEntity);
                context.SaveChanges();

                return vendorEntity.VendorId;
            }
        }

        public void DeleteVendor(int vendorId)
        {
            using (var context = _context.Create())
            {
                var vendor = context.Vendors.SingleOrDefault(v => v.VendorId == vendorId);
                if (vendor == null) 
                    throw new ArgumentException(string.Format("cannot find vendorId: '{0}'", vendorId));

                if (vendor.Users != null)
                    foreach (var user in vendor.Users.ToList())
                    {
                        vendor.Users.Remove(user);
                    }

                if (vendor.Applications != null)
                    foreach (var app in vendor.Applications.ToList())
                    {
                        RemoveApplicationHierarchy(context, app);
                    }

                context.Vendors.Remove(vendor);
                context.SaveChanges();
            }
        }

        public void UpdateVendor(VendorModel vendor)
        {
            using (var context = _context.Create())
            {
                var vendorEntity = context.Vendors.SingleOrDefault(v => v.VendorId == vendor.VendorId);
                if (vendorEntity == null)
                    throw new ArgumentException(string.Format("cannot find vendor id '{0}'", vendor.VendorId));

                var mainUser = vendorEntity.Users.FirstOrDefault();
                if (mainUser == null)
                {
                    mainUser = new User();
                    vendorEntity.Users.Add(mainUser);
                }

                // sanitize data:
                vendor.VendorName = vendor.VendorName.Trim();
                vendor.NamespacePrefix = vendor.NamespacePrefix == null ? null : vendor.NamespacePrefix.Trim();
                vendor.MainContact.ContactName = vendor.MainContact.ContactName.Trim();
                vendor.MainContact.ContactEmailAddress = vendor.MainContact.ContactEmailAddress.Trim();

                // data validation:
                if (context.Vendors.Any(v =>
                    v.VendorName == vendor.VendorName &&
                    v.VendorId != vendor.VendorId))
                {
                    throw new DuplicateNameException(string.Format("Vendor '{0}' already exists", vendor.VendorName));
                }

                // prepare entities:
                vendorEntity.VendorName = vendor.VendorName;
                vendorEntity.NamespacePrefix = vendor.NamespacePrefix;
                mainUser.FullName = vendor.MainContact.ContactName;
                mainUser.Email = vendor.MainContact.ContactEmailAddress;

                context.SaveChanges();
            }

        }
    }
}
