using AutoMapper;
using LogiTrack.Entities;
using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Controllers
{
    [Route("api/company")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [Authorize]
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

            return Ok(company);
        }

        [HttpPost]
        public ActionResult CreateCompany([FromBody] CreateCompanyDto dto)
        {
            var id = _companyService.CreateCompany(dto);

            return Created($"/api/company/{id}", null);
        }

        [HttpPut("{id}")]
        public ActionResult UpdateCompany([FromBody] UpdateCompanyDto dto, [FromRoute] int id)
        {
            _companyService.UpdateCompany(id, dto);

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteCompany([FromRoute] int id)
        {
            _companyService.DeleteCompany(id);

            return NoContent();
        }
    }
}
