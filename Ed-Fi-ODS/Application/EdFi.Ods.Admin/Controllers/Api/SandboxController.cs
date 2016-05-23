using EdFi.Ods.Admin.Models;
using System.Web.Http;

namespace EdFi.Ods.Admin.Controllers.Api
{
    public class SandboxDTO
    {
        public string Command { get; set; }
    }

    [Authorize(Roles="Administrator")]
    public class SandboxController : ApiController
    {
        private readonly ISandboxProvisioner _sandboxProvisioner;

        public SandboxController(ISandboxProvisioner sandboxProvisioner)
        {
            _sandboxProvisioner = sandboxProvisioner;
        }

        /// <summary>
        /// Perform an operation on the shared sandbox
        /// </summary>
        /// <param name="sandboxDTO">"reset" is the only current option</param>
        public void Put(SandboxDTO sandboxDTO)
        {
            switch (sandboxDTO.Command)
            {
                case "reset":
                    _sandboxProvisioner.ResetDemoSandbox();
                    break;
            } 
        }

    }
}
