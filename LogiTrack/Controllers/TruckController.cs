using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [Route("/api/truck")]
    public class TruckController : ControllerBase
    {
        private readonly ITruckService _truckService;

        public TruckController(ITruckService truckService)
        {
            _truckService = truckService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TruckDto>> GetAllTrucks()
        {
            var trucksDtos = _truckService.GetAll();

            return Ok(trucksDtos);
        }

        [HttpGet("{id}")]
        public ActionResult<TruckDto> GetTruckById([FromRoute] int id)
        {
            var truck = _truckService.GetById(id);

            if (truck is null)
            {
                return NotFound();
            }

            return Ok(truck);
        }

        [HttpPost]
        public ActionResult CreateTruck([FromBody] CreateTruckDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = _truckService.CreateTruck(dto);

            return Created($"/api/truck/{id}", null);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateTruck([FromBody] UpdateTruckDto dto, [FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isUpdated = _truckService.UpdateTruck(id, dto);

            if (!isUpdated)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteTruck([FromRoute] int id)
        {
            var isDeleted = _truckService.DeleteTruck(id);

            if (isDeleted)
            {
                return NoContent();
            }

            return NotFound();
        }

    }
}
