using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [Route("/api/driver")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<DriverDto>> GetAllDrivers()
        {
            var driversDtos = _driverService.GetAll();

            return Ok(driversDtos);
        }

        [HttpGet("{id}")]
        public ActionResult<DriverDto> GetDriverById([FromRoute] int id)
        {
            var driver = _driverService.GetById(id);
            
            return Ok(driver);
        }

        [HttpPost]
        public ActionResult CreateDriver([FromBody] CreateDriverDto dto)
        {
            var id = _driverService.CreateDriver(dto);

            return Created($"/api/driver/{id}", null);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateDriver([FromBody] UpdateDriverDto dto, [FromRoute] int id)
        {
            _driverService.UpdateDriver(id, dto);

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteDriver([FromRoute] int id)
        {
            _driverService.DeleteDriver(id);

            return NoContent();
        }
    }
}
