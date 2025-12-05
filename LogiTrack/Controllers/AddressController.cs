using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [Route("api/address")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<AddressDto>> GetAllAddresses()
        {
            var addressesDtos = _addressService.GetAll();

            return Ok(addressesDtos);
        }

        [HttpGet("{id}")]
        public ActionResult<AddressDto> GetAddressById([FromRoute] int id)
        {
            var address = _addressService.GetById(id);

            return Ok(address);
        }

        [HttpPost]
        public ActionResult CreateAddress([FromBody] CreateAddressDto dto)
        {
            var id = _addressService.CreateAddress(dto);

            return Created($"/api/address/{id}", null);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateAddress([FromBody]UpdateAddressDto dto, [FromRoute] int id)
        {
            _addressService.UpdateAddress(id, dto);

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteAddress([FromRoute] int id)
        {
            _addressService.DeleteAdderss(id);

            return NoContent();
        }
    }
}
