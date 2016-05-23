using System;
using System.Data;
using System.Web.Http;
using EdFi.Ods.SecurityConfiguration.Services;
using EdFi.Ods.SecurityConfiguration.Services.Model;
using log4net;

namespace EdFi.Ods.SecurityConfiguration.Core.Controllers.Administration
{
    [Authorize]
    public class ApplicationController : ApiController
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(ApplicationController));

        private readonly IApplicationService _applicationService;

        public ApplicationController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [Route("api/vendors/{vendorId}/applications")]
        public IHttpActionResult Get(int vendorId)
        {
            var result = _applicationService.GetVendorApplications(vendorId);

            return Ok(result);
        }

        [Route("api/vendors/{vendorId}/applications/{applicationId}")]
        public IHttpActionResult GetById(int vendorId, int applicationId)
        {
            var result = _applicationService.GetById(vendorId, applicationId);

            return Ok(result);
        }

        [Route("api/vendors/{vendorId}/applications")]
        public IHttpActionResult Post(int vendorId, Application newApplication)
        {
            // todo: server validation for newApplication should be implemented here
            // example of server validation
            if(string.IsNullOrWhiteSpace(newApplication.ApplicationName))
                return BadRequest("Application name should be valid");

            try
            {
                return Ok(new {applicationId = _applicationService.AddApplication(vendorId, newApplication)});
            }
            catch (DuplicateNameException e)
            {
                return BadRequest(e.Message);
            }
            catch(Exception e)
            {
                _logger.Debug(string.Format("Error creating new application {{ vendorId: {0}, applicationName: {1}}}",
                    vendorId, newApplication.ApplicationName), e);
                
                return InternalServerError();
            }
        }

        [Route("api/vendors/{vendorId}/applications/{applicationId}")]
        public IHttpActionResult Delete(int vendorId, int applicationId)
        {
            try
            {
                _applicationService.DeleteApplication(vendorId, applicationId);
                return Ok();
            }
            catch(Exception e)
            {
                _logger.Debug(string.Format("Error deleting application {{ vendorId: {0}, applicationId: {1}}}",
                    vendorId, applicationId), e);

                return InternalServerError();
            }

        }

        [Route("api/vendors/{vendorId}/applications")]
        public IHttpActionResult Put(int vendorId, Application updatingApplication)
        {
            try
            {
                _applicationService.UpdateApplication(vendorId, updatingApplication);
                return Ok();
            }
            catch (DuplicateNameException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.Debug(string.Format("Error updating application {{ vendorId: {0}, applicationId: {1}}}",
                    vendorId, updatingApplication.ApplicationId), e);
                
                return InternalServerError();
            }
        }

        [Route("api/vendors/{vendorId}/applications/{applicationId}/regen")]
        public IHttpActionResult PutKeyGen(int vendorId, int applicationId)
        {
            try
            {
                return Ok(_applicationService.GenerateApplicationKey(vendorId, applicationId));
            }
            catch (Exception e)
            {
                _logger.Debug(
                    string.Format("Error generating key for application {{ vendorId: {0}, applicationId: {1}}}",
                        vendorId, applicationId), e);

                return InternalServerError();
            }
        }
    }
}
