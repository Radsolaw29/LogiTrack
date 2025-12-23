using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [Route("/api/company/{companyId}/truck")]
    [ApiController]
    [Authorize]
    public class TruckController : ControllerBase
    {
        private readonly ITruckService _truckService;

        public TruckController(ITruckService truckService)
        {
            _truckService = truckService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<IEnumerable<TruckDto>> GetAllTrucks([FromRoute] int companyId, [FromQuery] TruckQuery? query)
        {
            var trucksDtos = _truckService.GetAll(companyId, query);

            return Ok(trucksDtos);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<TruckDto> GetTruckById([FromRoute] int companyId, [FromRoute] int id)
        {
            var truck = _truckService.GetById(companyId, id);

            return Ok(truck);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult CreateTruck([FromRoute] int companyId, [FromBody] CreateTruckDto dto)
        {
            var id = _truckService.CreateTruck(companyId, dto);

            return Created($"/api/company/{companyId}/truck/{id}", null);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult UpdateTruck([FromRoute] int companyId, [FromBody] UpdateTruckDto dto, [FromRoute] int id)
        {
            _truckService.UpdateTruck(companyId, id, dto);

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteTruck([FromRoute] int companyId, [FromRoute] int id)
        {
            _truckService.DeleteTruck(companyId, id);

            return NoContent();
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteAllTrucks([FromRoute] int companyId)
        {
            _truckService.DeleteAllTrucks(companyId);

            return NoContent();
        }
    }
}
