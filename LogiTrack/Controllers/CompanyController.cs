using LogiTrack.Interfaces;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers
{
    [Route("api/company")]
    [ApiController]
    [Authorize]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }
        
        [HttpGet]
        [Authorize(Roles ="Admin,Manager")]
        public ActionResult<IEnumerable<CompanyDto>> GetAllCompanies([FromQuery] CompanyQuery? query)
        {
            var companiesDtos = _companyService.GetAll(query);

            return Ok(companiesDtos);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<CompanyDto> GetCompany([FromRoute] int id)
        {
            var company = _companyService.GetById(id);

            return Ok(company);
        }

        [HttpPost]
        [Authorize(Roles ="Admin")]
        public ActionResult CreateCompany([FromBody] CreateCompanyDto dto)
        { 
            var id = _companyService.CreateCompany(dto);

            return Created($"/api/company/{id}", null);
        }

        [HttpPut("{id}")]
        [Authorize(Roles ="Admin,Manager")]
        public ActionResult UpdateCompany([FromBody] UpdateCompanyDto dto, [FromRoute] int id)
        {
            _companyService.UpdateCompany(id, dto);

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteCompany([FromRoute] int id)
        {
            _companyService.DeleteCompany(id);

            return NoContent();
        }
    }
}