using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [Route("/api/company/{companyId}/transportOrder")]
    [ApiController]
    [Authorize]
    public class TransportOrderController : ControllerBase
    {
        private readonly ITransportOrderService _transportOrderService;

        public TransportOrderController(ITransportOrderService transportOrderService)
        {
            _transportOrderService = transportOrderService;
        }

        [HttpGet]
        [Authorize(Roles ="Admin,Manager")]
        public ActionResult<IEnumerable<TransportOrderDto>> GetAllTransportOrder([FromRoute] int companyId, [FromQuery] TransportOrderQuery? query)
        {
            var transportOrders = _transportOrderService.GetAllTransportOrders(companyId, query);

            return Ok(transportOrders);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<TransportOrderDto> GetTransportOrderById([FromRoute] int companyId, [FromRoute] int id)
        {
            TransportOrderDto transportOrder = _transportOrderService.GetTransportOrderById(companyId, id);

            return Ok(transportOrder);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult CreateTransportOrder([FromRoute] int companyId ,[FromBody] CreateTransportOrderDto dto)
        {
            var id = _transportOrderService.CreateTransportOrder(companyId ,dto);

            return Created($"/api/company/{companyId}/transportOrder/{id}", null);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult UpdateTransportOrder([FromRoute] int companyId, [FromBody] UpdateTransportOrder dto, [FromRoute] int id)
        {
            _transportOrderService.UpdateTransportOrder(companyId ,id, dto);

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteTransportOrder([FromRoute] int companyId, [FromRoute] int id)
        {
            _transportOrderService.DeleteTransportOrder(companyId, id);

            return NoContent();
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteAllTransportOrders([FromRoute] int companyId)
        {
            _transportOrderService.DeleteAllTraansportOrders(companyId);

            return NoContent();
        }
    }
}