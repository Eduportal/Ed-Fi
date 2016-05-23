using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.Admin.Models.Sandbox;
using EdFi.Ods.Common.Utils;

namespace EdFi.Ods.Admin.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SandboxController : Controller
    {
        private readonly ISandboxProvisioner _sandboxProvisioner;
        private readonly IClientAppRepo _clientAppRepo;

        public SandboxController(ISandboxProvisioner sandboxProvisioner, IClientAppRepo clientAppRepo)
        {
            _sandboxProvisioner = sandboxProvisioner;
            _clientAppRepo = clientAppRepo;
        }

        [HttpGet]
        public ActionResult Orphans()
        {
            var model = GetSandboxIndexViewModel();
            return View(model);
        }

        private SandboxIndexViewModel GetSandboxIndexViewModel()
        {
            var users = _clientAppRepo.GetUsers();
            var knownSandboxes = new List<SandboxViewModel>();
            foreach (var user in users)
            {
                foreach (var client in user.ApiClients)
                    knownSandboxes.Add(new SandboxViewModel
                                           {
                                               Client = client.Name,
                                               User = user.Email,
                                               ApplicationName = client.Application.ApplicationName,
                                               Sandbox = DatabaseNameBuilder.SandboxNameForKey(client.Key)
                                           });
            }
            var sandboxDatabases = _sandboxProvisioner.GetSandboxDatabases();
            var orphans = sandboxDatabases.Where(db => knownSandboxes.All(k => k.Sandbox != db)).ToArray();

            var ownedSandboxes = knownSandboxes.Where(sb => sandboxDatabases.Contains(sb.Sandbox)).ToArray();
            var missingSandboxes = knownSandboxes.Except(ownedSandboxes).ToArray();

            var model = new SandboxIndexViewModel
                            {
                                AllSandboxes = sandboxDatabases,
                                OwnedSandboxes = ownedSandboxes,
                                OrphanSandboxes = orphans,
                                MissingSandboxes = missingSandboxes
                            };
            return model;
        }

        [HttpPost]
        public ActionResult RemoveOrphans()
        {
            var message = "success";
            var success = true;
            try
            {
                var orphanKeys = GetSandboxIndexViewModel()
                    .OrphanSandboxes
                    .Select(DatabaseNameBuilder.KeyFromSandboxName)
                    .ToArray();

                _sandboxProvisioner.DeleteSandboxes(orphanKeys);
            }
            catch (Exception e)
            {
                message = e.Message;
                success = false;
                //GULP.  
                //  Exception swallowed since we just reload the page either way.  We aren't showing error messages here right now.
                //  If we start showing error messages, we should either handled exceptions or stop catching them.
            }
            return Json(new {success, message});
        }
    }
}