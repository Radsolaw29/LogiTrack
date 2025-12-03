using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [Route("/api/driver")]
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

            if(driver is null)
            {
                return NotFound();
            }

            return Ok(driver);
        }

        [HttpPost]
        public ActionResult CreateDriver([FromBody] CreateDriverDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = _driverService.CreateDriver(dto);

            return Created($"/api/driver/{id}", null);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateDriver([FromBody] UpdateDriverDto dto, [FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isUpdated = _driverService.UpdateDriver(id, dto);

            if (!isUpdated)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteDriver([FromRoute] int id)
        {
            var isDeleted = _driverService.DeleteDriver(id);

            if (isDeleted)
            {
                return NoContent();
            }

            return NotFound();
        }
    }
}
