using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [Route("api/address")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult<IEnumerable<AddressDto>> GetAllAddresses([FromQuery] AddressQuery? query)
        {
            var addressesDtos = _addressService.GetAll(query);

            return Ok(addressesDtos);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<AddressDto> GetAddressById([FromRoute] int id)
        {
            var address = _addressService.GetById(id);

            return Ok(address);
        }

        [HttpPost]
        [Authorize(Roles="Admin")]
        public ActionResult CreateAddress([FromBody] CreateAddressDto dto)
        {
            var id = _addressService.CreateAddress(dto);

            return Created($"/api/address/{id}", null);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult UpdateAddress([FromBody]UpdateAddressDto dto, [FromRoute] int id)
        {
            _addressService.UpdateAddress(id, dto);

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteAddress([FromRoute] int id)
        {
            _addressService.DeleteAdderss(id);

            return NoContent();
        }
    }
}