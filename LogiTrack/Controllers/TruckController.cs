using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [Route("/api/company/{companyId}/truck")]
    [ApiController]
    public class TruckController : ControllerBase
    {
        private readonly ITruckService _truckService;

        public TruckController(ITruckService truckService)
        {
            _truckService = truckService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TruckDto>> GetAllTrucks([FromRoute] int companyId, [FromQuery] TruckQuery? query)
        {
            var trucksDtos = _truckService.GetAll(companyId, query);

            return Ok(trucksDtos);
        }

        [HttpGet("{id}")]
        public ActionResult<TruckDto> GetTruckById([FromRoute] int companyId, [FromRoute] int id)
        {
            var truck = _truckService.GetById(companyId, id);

            return Ok(truck);
        }

        [HttpPost]
        public ActionResult CreateTruck([FromRoute] int companyId, [FromBody] CreateTruckDto dto)
        {
            var id = _truckService.CreateTruck(companyId, dto);

            return Created($"/api/company/{companyId}/truck/{id}", null);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateTruck([FromRoute] int companyId, [FromBody] UpdateTruckDto dto, [FromRoute] int id)
        {
            _truckService.UpdateTruck(companyId, id, dto);

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteTruck([FromRoute] int companyId, [FromRoute] int id)
        {
            _truckService.DeleteTruck(companyId, id);

            return NoContent();
        }

        [HttpDelete]
        public ActionResult DeleteAllTrucks([FromRoute] int companyId)
        {
            _truckService.DeleteAllTrucks(companyId);

            return NoContent();
        }
    }
}
