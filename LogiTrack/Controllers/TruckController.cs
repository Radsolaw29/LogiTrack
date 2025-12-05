using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [Route("/api/truck")]
    [ApiController]
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

            return Ok(truck);
        }

        [HttpPost]
        public ActionResult CreateTruck([FromBody] CreateTruckDto dto)
        {
            var id = _truckService.CreateTruck(dto);

            return Created($"/api/truck/{id}", null);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateTruck([FromBody] UpdateTruckDto dto, [FromRoute] int id)
        {
            _truckService.UpdateTruck(id, dto);

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteTruck([FromRoute] int id)
        {
            _truckService.DeleteTruck(id);

            return NoContent();
        }
    }
}
