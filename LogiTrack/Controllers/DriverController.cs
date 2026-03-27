using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [Route("/api/company/{companyId}/driver")]
    [ApiController]
    [Authorize]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<IEnumerable<DriverDto>> GetAllDrivers([FromRoute] int companyId, [FromQuery] DriverQuery? query)
        {
            var driversDtos = _driverService.GetAll(companyId, query);

            return Ok(driversDtos);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<DriverDto> GetDriverById([FromRoute] int companyId, [FromRoute] int id)
        {
            DriverDto driver = _driverService.GetById(companyId, id);
            
            return Ok(driver);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult CreateDriver([FromRoute] int companyId ,[FromBody] CreateDriverDto dto)
        {
            var id = _driverService.CreateDriver(companyId, dto);

            return Created($"/api/company/{companyId}/driver/{id}", null);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult UpdateDriver([FromRoute] int companyId, [FromBody] UpdateDriverDto dto, [FromRoute] int id)
        {
            _driverService.UpdateDriver(companyId, id, dto);

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteDriver([FromRoute] int companyId, [FromRoute] int id)
        {
            _driverService.DeleteDriver(companyId, id);

            return NoContent();
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteAllDrivers([FromRoute] int companyId)
        {
            _driverService.DeleteAllDrivers(companyId);

            return NoContent();
        }
    }
}