using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [Route("/api/company/{companyId}/driver")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<DriverDto>> GetAllDrivers([FromRoute] int companyId)
        {
            var driversDtos = _driverService.GetAll(companyId);

            return Ok(driversDtos);
        }

        [HttpGet("{id}")]
        public ActionResult<DriverDto> GetDriverById([FromRoute] int companyId, [FromRoute] int id)
        {
            DriverDto driver = _driverService.GetById(companyId, id);
            
            return Ok(driver);
        }

        [HttpPost]
        public ActionResult CreateDriver([FromRoute] int companyId ,[FromBody] CreateDriverDto dto)
        {
            var id = _driverService.CreateDriver(companyId, dto);

            return Created($"/api/company/{companyId}/driver/{id}", null);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateDriver([FromRoute] int companyId, [FromBody] UpdateDriverDto dto, [FromRoute] int id)
        {
            _driverService.UpdateDriver(companyId, id, dto);

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteDriver([FromRoute] int companyId, [FromRoute] int id)
        {
            _driverService.DeleteDriver(companyId, id);

            return NoContent();
        }

        [HttpDelete]
        public ActionResult DeleteAllDrivers([FromRoute] int companyId)
        {
            _driverService.DeleteAllDrivers(companyId);

            return NoContent();
        }
    }
}
