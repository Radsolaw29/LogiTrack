using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [Route("/api/transportOrder")]
    public class TransportOrderController : ControllerBase
    {
        private readonly ITransportOrderService _transportOrderService;

        public TransportOrderController(ITransportOrderService transportOrderService)
        {
            _transportOrderService = transportOrderService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TransportOrderDto>> GetAllTransportOrder()
        {
            var transportOrders = _transportOrderService.GetAllTransportOrders();

            return Ok(transportOrders);
        }

        [HttpGet("{id}")]
        public ActionResult<TransportOrderDto> GetTransportOrderById([FromRoute] int id)
        {
            var transportOrder = _transportOrderService.GetTransportOrderById(id);

            if (transportOrder is null)
            {
                return NotFound();
            }

            return Ok(transportOrder);
        }

        [HttpPost]
        public ActionResult CreateTransportOrder([FromBody] CreateTransportOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = _transportOrderService.CreateTransportOrder(dto);

            return Created($"/api/transportOrder/{id}", null);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateTransportOrder([FromBody] UpdateTransportOrder dto, [FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isUpdated = _transportOrderService.UpdateTransportOrder(id, dto);

            if (!isUpdated)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteTransportOrder([FromRoute] int id)
        {
            var isDeleted = _transportOrderService.DeleteTransportOrder(id);

            if (isDeleted)
            {
                return NoContent();
            }

            return NotFound();
        }
    }
}
