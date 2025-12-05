using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [Route("/api/transportOrder")]
    [ApiController]
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

            return Ok(transportOrder);
        }

        [HttpPost]
        public ActionResult CreateTransportOrder([FromBody] CreateTransportOrderDto dto)
        {
            var id = _transportOrderService.CreateTransportOrder(dto);

            return Created($"/api/transportOrder/{id}", null);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateTransportOrder([FromBody] UpdateTransportOrder dto, [FromRoute] int id)
        {
            _transportOrderService.UpdateTransportOrder(id, dto);

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteTransportOrder([FromRoute] int id)
        {
            _transportOrderService.DeleteTransportOrder(id);

            return NoContent();
        }
    }
}
