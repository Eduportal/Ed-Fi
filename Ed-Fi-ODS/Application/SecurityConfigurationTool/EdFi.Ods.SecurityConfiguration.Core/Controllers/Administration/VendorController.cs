using System;
using System.Data;
using System.Linq;
using System.Web.Http;
using EdFi.Ods.SecurityConfiguration.Services;
using EdFi.Ods.SecurityConfiguration.Services.Model;
using log4net;

namespace EdFi.Ods.SecurityConfiguration.Core.Controllers.Administration
{
    [Authorize]
    public class VendorController : ApiController
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(VendorController));

        private readonly IVendorService _vendorService;

        public VendorController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        [Route("api/vendors")]
        public IHttpActionResult Get()
        {
            try
            {
                var result = _vendorService.GetAll().ToList();
                return Ok(result);
            }
            catch(Exception e)
            {
                _logger.Debug("Error listing vendors", e);

                return InternalServerError();
            }
        }

        [Route("api/vendors/{vendorId}")]
        public IHttpActionResult Get(int vendorId)
        {
            try
            {
                var result = _vendorService.GetById(vendorId);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception)
            {
                return InternalServerError();
            }
        }

        [Route("api/vendors/{vendorId}")]
        public IHttpActionResult Delete(int vendorId)
        {
            try
            {
                _vendorService.DeleteVendor(vendorId);
                return Ok();
            }
            catch(Exception e)
            {
                _logger.Debug(string.Format("Error deleting vendor {{ vendorId: {0}}}",
                    vendorId), e);

                return InternalServerError();
            }
        }

        [Route("api/vendors")]
        public IHttpActionResult Post(Vendor vendor)
        {
            try
            {
                return Ok(new {vendorId = _vendorService.AddVendor(vendor)});
            }
            catch (DuplicateNameException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.Debug(string.Format("Error creating new vendor {{ vendorName: {0}}}",
                    vendor.VendorName), e);

                return InternalServerError();
            }
        }

        [Route("api/vendors")]
        public IHttpActionResult Put(Vendor vendor)
        {
            try
            {
                _vendorService.UpdateVendor(vendor);
                return Ok();
            }
            catch (DuplicateNameException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.Debug(string.Format("Error updating vendor {{ vendorId: {0}}}",
                    vendor.VendorId), e);

                return InternalServerError();
            }
        }
    }
}
