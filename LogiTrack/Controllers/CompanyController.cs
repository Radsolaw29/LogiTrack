using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Controllers
{
    [Route("api/company")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CompanyDto>> GetAllCompanies()
        {
            var companiesDtos = _companyService.GetAll();

            return Ok(companiesDtos);
        }

        [HttpGet("{id}")]
        public ActionResult<CompanyDto> GetCompany([FromRoute] int id)
        {
            var company = _companyService.GetById(id);

            if (company is null)
            {
                return NotFound();
            }

            return Ok(company);
        }

        [HttpPost]
        public ActionResult CreateCompany([FromBody] CreateCompanyDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var id = _companyService.CreateCompany(dto);

            return Created($"/api/company/{id}", null);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateCompany([FromBody] UpdateCompanyDto dto, [FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isUpdated = _companyService.UpdateCompany(id, dto);

            if(!isUpdated) return NotFound();

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteCompany([FromRoute] int id)
        {
            var isDeleted = _companyService.DeleteCompany(id);

            if (isDeleted)
            {
                return NoContent();
            }

            return NotFound();
        }
    }
}
